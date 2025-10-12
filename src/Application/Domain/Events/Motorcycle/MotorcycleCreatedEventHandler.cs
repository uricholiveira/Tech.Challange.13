using Domain.Models.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.Interfaces;

namespace Domain.Events.Motorcycle;

public class MotorcycleCreatedEventHandler(
    ILogger<MotorcycleCreatedEventHandler> logger,
    IRabbitMqClient rabbitMqClient
)
    : INotificationHandler<MotorcycleCreatedEvent>
{
    public async Task Handle(MotorcycleCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Evento de criação de moto recebido: {Id}", notification.Id);
        logger.LogInformation("Enviando evento para RabbitMQ");
        var result = await rabbitMqClient.PublishAsync("motorcycle", "motorcycle.created", notification);

        if (result.IsSuccess)
            logger.LogInformation("Evento enviado para RabbitMQ");
        else
            logger.LogError("Erro ao enviar evento para RabbitMQ");
    }
}