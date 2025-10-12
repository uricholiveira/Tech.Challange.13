using Domain.Entities;
using Domain.Models.Events;
using Domain.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared.Core.Abstracts;
using Shared.Core.Interfaces;

namespace Business.Workers.Consumers;

public class MotorcycleCreatedConsumer(
    ILogger<MotorcycleCreatedConsumer> logger,
    IServiceProvider serviceProvider,
    IRabbitMqClient rabbitMqClient)
    : ResilientRabbitMqConsumerService<MotorcycleCreatedEvent>(logger, serviceProvider, rabbitMqClient)
{
    protected override string QueueName => "motorcycle.created";

    protected override async Task ProcessMessageAsync(MotorcycleCreatedEvent message, CancellationToken stoppingToken)
    {
        Logger.LogInformation("Mensagem recebida: {Id}", message.Id);

        await using var scope = ServiceProvider.CreateAsyncScope();
        var motorcycleNotificationRepository =
            scope.ServiceProvider.GetRequiredService<IMotorcycleNotificationRepository>();

        Logger.LogInformation("Verificando se existe notificação pra moto: {Id}", message.Id);
        var notificationExists = await motorcycleNotificationRepository.ExistsAsync(x => x.MotorcycleId == message.Id);
        if (notificationExists.Value)
        {
            Logger.LogInformation("Notificação já existe pra moto: {Id}", message.Id);
            return;
        }

        Logger.LogInformation("Criando notificação pra moto: {Id}", message.Id);
        var notification = MotorcycleNotification.Create(message.Id, message.Year, message.Model, message.LicensePlate);
        if (notification.IsFailure)
        {
            Logger.LogError("Erro ao criar notificação pra moto: {Id}, {Error}", message.Id,
                notification.Error.Message);
            return;
        }

        await motorcycleNotificationRepository.AddAsync(notification.Value, stoppingToken);
        Logger.LogInformation("Notificação criada com sucesso pra moto: {Id}", message.Id);
    }
}