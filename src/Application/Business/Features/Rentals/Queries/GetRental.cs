using System.Text.Json.Serialization;
using Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.Helpers;

namespace Business.Features.Rentals.Queries;

public static class GetRental
{
    public sealed class Handler(ILogger<Handler> logger, IRentalRepository rentalRepository)
        : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Buscando plano de identificador: {Id}", command.Id);

            var rental =
                await rentalRepository.FirstOrDefaultAsync(x => x.Id == command.Id);
            if (rental.IsFailure) return Result.Failure<Response>(rental.Error);

            return Result.Success(new Response(rental.Value.Id, rental.Value.RentalPlan.DailyAmount,
                rental.Value.MotorcycleId, rental.Value.RiderId, rental.Value.StartDate,
                rental.Value.EndDate, rental.Value.ExpectedEndDate, rental.Value.ReturnDate)
            );
        }
    }

    public sealed record Command(
        Guid Id
    ) : IRequest<Result<Response>>;

    public sealed record Response(
        [property: JsonPropertyName("identificador")]
        Guid Id,
        [property: JsonPropertyName("valor_diaria")]
        decimal DailyAmount,
        [property: JsonPropertyName("entregador_id")]
        Guid RiderId,
        [property: JsonPropertyName("motor_id")]
        Guid MotorcycleId,
        [property: JsonPropertyName("data_inicio")]
        DateOnly StartDate,
        [property: JsonPropertyName("data_termino")]
        DateOnly EndDate,
        [property: JsonPropertyName("data_previsao_termino")]
        DateOnly ExpectedEndDate,
        [property: JsonPropertyName("data_devolucao")]
        DateOnly? ReturnDate
    );

    public class Validator : AbstractValidator<Command>;
}