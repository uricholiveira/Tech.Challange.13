using System.Diagnostics;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Wrap;
using Shared.Core.Interfaces;

namespace Shared.Core.Abstracts;

public abstract class ResilientRabbitMqConsumerService<T> : BackgroundService
{
    private readonly IRabbitMqClient _rabbitMqClient;
    private readonly AsyncPolicyWrap _resiliencyPolicy;
    protected readonly ILogger Logger;
    protected readonly IServiceProvider ServiceProvider;

    protected ResilientRabbitMqConsumerService(
        ILogger logger,
        IServiceProvider serviceProvider,
        IRabbitMqClient rabbitMqClient)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _rabbitMqClient = rabbitMqClient ?? throw new ArgumentNullException(nameof(rabbitMqClient));
        _resiliencyPolicy = CreateResiliencyPolicy();
    }

    protected abstract string QueueName { get; }
    protected virtual string ActivitySourceName => "Shared.Abstracts.ResilientRabbitMqConsumerService";
    protected virtual string ActivitySourceVersion => "1.0.0";
    protected virtual int RetryCount => 3;
    protected virtual int CircuitBreakerThreshold => 5;
    protected virtual int CircuitBreakerDurationMinutes => 2;
    protected virtual int DelayOnCircuitBreakerOpenSeconds => 30;
    protected virtual int ReconnectionDelaySeconds => 5;
    protected virtual ushort PrefetchCount => 1;
    protected virtual bool DeclareQueueOnStart => true;
    protected virtual bool QueueDurable => true;

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
            "Tentativa {Attempt} de processamento da mensagem RabbitMQ falhou. Tentando novamente em {Delay} segundos...",
            retryCount, timeSpan.TotalSeconds);
    }

    protected virtual void OnCircuitBreakerOpen(Exception exception, TimeSpan duration)
    {
        Logger.LogWarning(
            "Circuit breaker está aberto para fila RabbitMQ {QueueName}. Aguardando {Duration} segundos...",
            QueueName, duration.TotalSeconds);
    }

    protected virtual void OnCircuitBreakerReset()
    {
        Logger.LogInformation("Circuit breaker resetado para fila RabbitMQ {QueueName}. Retomando operação normal.",
            QueueName);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("{ServiceName} iniciado para fila RabbitMQ {QueueName}.", GetType().Name, QueueName);

        while (!stoppingToken.IsCancellationRequested)
            try
            {
                if (!_rabbitMqClient.IsConnected)
                {
                    Logger.LogWarning("RabbitMQ não conectado. Aguardando reconexão...");
                    await Task.Delay(TimeSpan.FromSeconds(ReconnectionDelaySeconds), stoppingToken);
                    continue;
                }

                if (DeclareQueueOnStart)
                {
                    var declareResult = await _rabbitMqClient.DeclareQueueAsync(QueueName, QueueDurable);
                    if (!declareResult.IsSuccess)
                    {
                        Logger.LogError("Falha ao declarar fila {QueueName}: {Error}", QueueName,
                            declareResult.Error.Message);
                        await Task.Delay(TimeSpan.FromSeconds(ReconnectionDelaySeconds), stoppingToken);
                        continue;
                    }
                }

                var consumeResult = await _rabbitMqClient.ConsumeAsync<T>(
                    QueueName,
                    async message => await HandleMessageWithResilience(message, stoppingToken),
                    PrefetchCount
                );

                if (!consumeResult.IsSuccess)
                {
                    Logger.LogError("Falha ao iniciar consumidor na fila {QueueName}: {Error}",
                        QueueName, consumeResult.Error.Message);
                    await Task.Delay(TimeSpan.FromSeconds(ReconnectionDelaySeconds), stoppingToken);
                    continue;
                }

                Logger.LogInformation("Consumidor ativo na fila {QueueName}. Aguardando mensagens...", QueueName);

                // Aguarda até o cancelamento - o consumidor continua rodando em background
                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                Logger.LogInformation("{ServiceName} foi cancelado.", GetType().Name);
                break;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex,
                    "Erro de conexão com RabbitMQ no {ServiceName}. Tentando reconectar em {Delay} segundos...",
                    GetType().Name, ReconnectionDelaySeconds);

                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(ReconnectionDelaySeconds), stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    Logger.LogInformation("Reconexão cancelada. Encerrando {ServiceName}.", GetType().Name);
                    break;
                }
            }
    }

    private async Task<bool> HandleMessageWithResilience(T message, CancellationToken stoppingToken)
    {
        using var activitySource = new ActivitySource(ActivitySourceName, ActivitySourceVersion);
        using var activity = activitySource.StartActivity(
            $"ProcessMessage.{QueueName}",
            ActivityKind.Consumer
        );

        activity?.SetTag("messaging.system", "rabbitmq");
        activity?.SetTag("messaging.destination", QueueName);
        activity?.SetTag("messaging.operation", "process");
        activity?.SetTag("service.name", GetType().Name);

        try
        {
            await _resiliencyPolicy.ExecuteAsync(async () => { await ProcessMessageAsync(message, stoppingToken); });

            activity?.SetStatus(ActivityStatusCode.Ok);
            return true;
        }
        catch (BrokenCircuitException ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, "Circuit breaker open");
            activity?.AddException(ex);

            Logger.LogWarning("Circuit breaker aberto para RabbitMQ. Mensagem será reprocessada mais tarde.");
            await Task.Delay(TimeSpan.FromSeconds(DelayOnCircuitBreakerOpenSeconds), stoppingToken);
            return false;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);

            Logger.LogError(ex, "Erro ao processar mensagem RabbitMQ do tipo {MessageType}", typeof(T).Name);
            return false;
        }
    }

    protected abstract Task ProcessMessageAsync(T message, CancellationToken stoppingToken);
}