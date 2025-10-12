using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Wrap;

namespace Shared.Core.Abstracts;

public abstract class OneTimeResilientBackgroundService : BackgroundService
{
    private readonly AsyncPolicyWrap _resiliencyPolicy;
    protected readonly ILogger Logger;
    protected readonly IServiceProvider ServiceProvider;

    protected OneTimeResilientBackgroundService(ILogger logger, IServiceProvider serviceProvider)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _resiliencyPolicy = CreateResiliencyPolicy();
    }

    protected virtual int RetryCount => 3;
    protected virtual int CircuitBreakerThreshold => 3;
    protected virtual int CircuitBreakerDurationMinutes => 1;
    protected virtual int DelayOnFailureSeconds => 30;
    protected virtual int MaxRetryAttempts => 10;
    protected virtual string ActivitySourceName => "Shared.Core.Abstracts.OneTimeResilientBackgroundService";
    protected virtual string ActivitySourceVersion => "1.0.0";

    private AsyncPolicyWrap CreateResiliencyPolicy()
    {
        var retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                RetryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                OnRetry
            );

        var circuitBreakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                CircuitBreakerThreshold,
                TimeSpan.FromMinutes(CircuitBreakerDurationMinutes),
                OnCircuitBreakerOpen,
                OnCircuitBreakerReset
            );

        return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy);
    }

    protected virtual void OnRetry(Exception exception, TimeSpan timeSpan, int retryCount, Context context)
    {
        Logger.LogWarning(exception,
            "Tentativa {Attempt} falhou. Tentando novamente em {Delay} segundos...",
            retryCount, timeSpan.TotalSeconds);
    }

    protected virtual void OnCircuitBreakerOpen(Exception exception, TimeSpan duration)
    {
        Logger.LogWarning("Circuit breaker está aberto. Aguardando {Duration} segundos antes da próxima tentativa...",
            duration.TotalSeconds);
    }

    protected virtual void OnCircuitBreakerReset()
    {
        Logger.LogInformation("Circuit breaker resetado. Retomando operação normal.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("{ServiceName} iniciado (execução única).", GetType().Name);

        var attemptCount = 0;
        var success = false;

        while (!stoppingToken.IsCancellationRequested && !success && attemptCount < MaxRetryAttempts)
        {
            using var activitySource = new ActivitySource(ActivitySourceName, ActivitySourceVersion);
            using var activity = activitySource.StartActivity(
                $"ProcessWork.{GetType().Name}"
            );

            activity?.SetTag("service.name", GetType().Name);
            activity?.SetTag("iteration.timestamp", DateTimeOffset.UtcNow.ToString("o"));

            attemptCount++;

            try
            {
                await _resiliencyPolicy.ExecuteAsync(async () => { await ProcessWorkAsync(stoppingToken); });

                success = true;
                activity?.SetStatus(ActivityStatusCode.Ok);
                Logger.LogInformation("{ServiceName} executado com sucesso na tentativa {Attempt}.",
                    GetType().Name, attemptCount);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                activity?.SetStatus(ActivityStatusCode.Ok, "Service cancelled");
                Logger.LogInformation("{ServiceName} foi cancelado.", GetType().Name);
                break;
            }
            catch (BrokenCircuitException ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, "Circuit breaker open");
                activity?.AddException(ex);

                Logger.LogWarning(
                    "Circuit breaker está aberto. Tentativa {Attempt} de {MaxAttempts}. Aguardando antes da próxima tentativa...",
                    attemptCount, MaxRetryAttempts);

                if (attemptCount < MaxRetryAttempts)
                    await Task.Delay(TimeSpan.FromSeconds(DelayOnFailureSeconds), stoppingToken);
            }
            catch (Exception ex)
            {
                activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                activity?.AddException(ex);

                Logger.LogError(ex, "Erro no processamento. Tentativa {Attempt} de {MaxAttempts}.",
                    attemptCount, MaxRetryAttempts);

                if (attemptCount < MaxRetryAttempts)
                    await Task.Delay(TimeSpan.FromSeconds(DelayOnFailureSeconds), stoppingToken);
            }
        }

        if (success)
            Logger.LogInformation("{ServiceName} finalizado com sucesso.", GetType().Name);
        else if (attemptCount >= MaxRetryAttempts)
            Logger.LogError("{ServiceName} falhou após {MaxAttempts} tentativas. Parando execução.",
                GetType().Name, MaxRetryAttempts);
    }

    /// <summary>
    ///     Método abstrato que deve ser implementado pelas classes filhas para definir o trabalho a ser executado
    /// </summary>
    /// <param name="stoppingToken">Token de cancelamento</param>
    protected abstract Task ProcessWorkAsync(CancellationToken stoppingToken);
}