using Domain.Models.Events;
using Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Domain.Events.Rental;

public class RentalReturnDateUpdatedEventHandler(
    ILogger<RentalReturnDateUpdatedEventHandler> logger,
    IRentalRepository rentalRepository
)
    : INotificationHandler<RentalReturnDateUpdatedEvent>
{
    private const decimal AdditionalDailyRate = 50.00m;

    public async Task Handle(RentalReturnDateUpdatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Evento de atualização de data de devolução do aluguel de identificador: {Id}",
            notification.Id);

        var rental = await rentalRepository.FirstOrDefaultAsync(x => x.Id == notification.Id);
        if (rental.IsFailure)
        {
            logger.LogCritical("Falha ao buscar aluguel com identificador: {Id}", notification.Id);
            return;
        }

        var actualReturnDate = notification.ReturnDate;
        var expectedEndDate = rental.Value.ExpectedEndDate;
        var rentalPlan = rental.Value.RentalPlan;

        var baseAmount = (decimal)rental.Value.ExpectedAmount!;

        decimal totalAmount;
        decimal penalty = 0;
        decimal additionalCharges = 0;

        if (actualReturnDate < expectedEndDate)
        {
            // Devolução antecipada - cobra multa sobre diárias não efetivadas
            var unusedDays = expectedEndDate.DayNumber - actualReturnDate.DayNumber;
            var unusedAmount = rentalPlan.DailyAmount * unusedDays;
            var penaltyPercentage = rentalPlan.PenaltyPercentage / 100m; // Converte 20.00 para 0.20

            penalty = unusedAmount * penaltyPercentage;
            totalAmount = baseAmount + penalty;

            logger.LogInformation(
                "Devolução antecipada. Aluguel: {RentalId}, Dias não utilizados: {UnusedDays}, Valor não utilizado: {UnusedAmount}, Multa: {Penalty} ({Percentage}%)",
                notification.Id, unusedDays, unusedAmount, penalty, rentalPlan.PenaltyPercentage);
        }
        else if (actualReturnDate > expectedEndDate)
        {
            // Devolução atrasada - cobra R$50 por dia adicional
            var extraDays = actualReturnDate.DayNumber - expectedEndDate.DayNumber;
            additionalCharges = AdditionalDailyRate * extraDays;
            totalAmount = baseAmount + additionalCharges;

            logger.LogInformation(
                "Devolução atrasada. Aluguel: {RentalId}, Dias extras: {ExtraDays}, Cobrança adicional: {AdditionalCharges}",
                notification.Id, extraDays, additionalCharges);
        }
        else
        {
            totalAmount = baseAmount;

            logger.LogInformation(
                "Devolução na data prevista. Aluguel: {RentalId}, Valor total: {TotalAmount}",
                notification.Id, totalAmount);
        }

        logger.LogInformation(
            "Cálculo final - Aluguel: {RentalId}, Plano: {Days} dias, Valor base: {BaseAmount}, Multa: {Penalty}, Adicional: {Additional}, Total: {TotalAmount}",
            notification.Id, rentalPlan.Days, baseAmount, penalty, additionalCharges, totalAmount);

        rental.Value.UpdateTotalAmountAndPenalty(totalAmount, penalty);
        await rentalRepository.UpdateAsync(rental.Value, cancellationToken);
    }
}