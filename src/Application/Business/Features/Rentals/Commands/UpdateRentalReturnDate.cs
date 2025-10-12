using Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.Helpers;

namespace Business.Features.Rentals.Commands;

public static class UpdateRentalReturnDate
{
    public sealed class Handler(
        ILogger<Handler> logger,
        IRentalRepository rentalRepository
    )
        : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Buscando plano de aluguel de identificador: {Id}", command.RentalPlanId);
            var rental =
                await rentalRepository.FirstOrDefaultAsync(x => x.Id == command.RentalPlanId);
            if (rental.IsFailure) return Result.Failure<Response>(rental.Error);

            if (rental.Value.ReturnDate.HasValue)
                return Result.Failure<Response>(Error.Validation("RENTAL.RETURN_DATE.ALREADY_SET",
                    "Data de retorno já foi definida"));

            logger.LogInformation("Atualizando data de retorno do plano de aluguel de identificador: {Id}",
                command.RentalPlanId);
            var result = rental.Value.SetReturnDate(command.ReturnDate);
            if (result.IsFailure) return Result.Failure<Response>(result.Error);

            await rentalRepository.UpdateAsync(rental.Value, cancellationToken);
            return Result.Success(new Response());
        }
    }

    public sealed record Command(
        Guid RentalPlanId,
        DateOnly ReturnDate
    ) : IRequest<Result<Response>>;

    public sealed record Response;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.ReturnDate)
                .GreaterThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("Data de retorno deve ser posterior ou igual a {ComparisonValue}");
        }
    }
}