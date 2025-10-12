using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Core.Abstracts;
using Shared.Core.Interfaces;

namespace Business.Workers.Jobs;

public class EnsureRabbitMqJob(ILogger<EnsureRabbitMqJob> logger, IServiceProvider serviceProvider)
    : OneTimeResilientBackgroundService(logger, serviceProvider)
{
    protected override async Task ProcessWorkAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Configurando infraestrutura do RabbitMQ");

        await using var scope = ServiceProvider.CreateAsyncScope();
        var rabbitMqClient = scope.ServiceProvider.GetRequiredService<IRabbitMqClient>();

        var exchangeResult = await rabbitMqClient.DeclareExchangeAsync("motorcycle", "topic");
        if (!exchangeResult.IsSuccess)
        {
            Logger.LogError("Erro ao declarar exchange: {Error}", exchangeResult.Error.Message);
            throw new Exception($"Falha ao configurar exchange: {exchangeResult.Error.Message}");
        }

        var queueResult = await rabbitMqClient.DeclareQueueAsync("motorcycle.created");
        if (!queueResult.IsSuccess)
        {
            Logger.LogError("Erro ao declarar fila: {Error}", queueResult.Error.Message);
            throw new Exception($"Falha ao configurar fila: {queueResult.Error.Message}");
        }

        var bindResult = await rabbitMqClient.BindQueueAsync(
            "motorcycle.created",
            "motorcycle",
            "motorcycle.created"
        );
        if (!bindResult.IsSuccess)
        {
            Logger.LogError("Erro ao fazer bind: {Error}", bindResult.Error.Message);
            throw new Exception($"Falha ao fazer bind: {bindResult.Error.Message}");
        }

        Logger.LogInformation("Infraestrutura do RabbitMQ configurada com sucesso");
    }
}