using Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.Helpers;

namespace Business.Features.Motorcycles.Commands;

public static class DeleteMotorcycle
{
    public sealed class Handler(
        ILogger<Handler> logger,
        IMotorcycleRepository motorcycleRepository,
        IRentalRepository rentalRepository
    )
        : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Buscando moto de identificador: {Identifier}", command.Identifier);

            var motorcycle =
                await motorcycleRepository.FirstOrDefaultAsync(x => x.Identifier == command.Identifier);
            if (motorcycle.IsFailure) return Result.Failure<Response>(motorcycle.Error);

            var rentalExists = await rentalRepository.ExistsAsync(x => x.MotorcycleId == motorcycle.Value.Id);
            if (rentalExists.Value)
                return Result.Failure<Response>(Error.Conflict("MOTORCYCLE.ALREADY_RENTED", "Moto já alugada"));

            logger.LogInformation("Removendo moto de identificador: {Identifier}", command.Identifier);
            var result = await motorcycleRepository.RemoveAsync(motorcycle.Value, cancellationToken);

            return result.IsFailure ? Result.Failure<Response>(result.Error) : Result.Success(new Response());
        }
    }

    public sealed record Command(
        string Identifier
    ) : IRequest<Result<Response>>;

    public sealed record Response;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Identifier)
                .NotEmpty()
                .WithMessage("O identificador é obrigatório");
        }
    }
}