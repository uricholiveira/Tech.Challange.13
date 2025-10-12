using Domain.Models.Events;
using Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Domain.Events.Rental;

public class RentalCreatedEventHandler(
    ILogger<RentalCreatedEventHandler> logger,
    IRentalRepository rentalRepository,
    IRentalPlanRepository rentalPlanRepository
)
    : INotificationHandler<RentalCreatedEvent>
{
    public async Task Handle(RentalCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Evento de criação de aluguel de identificador: {Id}",
            notification.Id);

        var rentalPlan = await rentalPlanRepository.FirstOrDefaultAsync(x => x.Id == notification.RentalPlanId);
        if (rentalPlan.IsFailure)
        {
            logger.LogCritical("Falha ao buscar plano de aluguel com identificador: {Id}", notification.RentalPlanId);
            // TODO: Throw or treat error
            return;
        }

        var rental = await rentalRepository.FirstOrDefaultAsync(x => x.Id == notification.Id);
        if (rental.IsFailure)
        {
            logger.LogCritical("Falha ao buscar aluguel com identificador: {Id}", notification.Id);
            return;
        }

        // Calcula a quantidade de dias entre StartDate e ExpectedEndDate (inclusivo)
        var totalDays = rental.Value.ExpectedEndDate.DayNumber - rental.Value.StartDate.DayNumber + 1;
        var totalAmount = rentalPlan.Value.DailyAmount * totalDays;

        logger.LogInformation("Valor total calculado para o aluguel {RentalId}: {TotalAmount} ({Days} dias)",
            notification.Id, totalAmount, totalDays);

        rental.Value.UpdateExpectedAmount(totalAmount);
        await rentalRepository.UpdateAsync(rental.Value, cancellationToken);
    }
}