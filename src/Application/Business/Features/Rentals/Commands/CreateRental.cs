using Domain.Entities;
using Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.Helpers;

namespace Business.Features.Rentals.Commands;

public static class CreateRental
{
    public sealed class Handler(
        ILogger<Handler> logger,
        IRentalRepository rentalRepository,
        IRentalPlanRepository rentalPlanRepository,
        IMotorcycleRepository motorcycleRepository,
        IRiderRepository riderRepository
    )
        : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Buscando moto de identificador: {Identifier}", command.MotorcycleIdentifier);
            var motorcycle =
                await motorcycleRepository.FirstOrDefaultAsync(x => x.Identifier == command.MotorcycleIdentifier);
            if (motorcycle.IsFailure) return Result.Failure<Response>(motorcycle.Error);

            logger.LogInformation("Buscando entregador de identificador: {Identifier}", command.RiderIdentifier);
            var rider = await riderRepository.FirstOrDefaultAsync(x => x.Identifier == command.RiderIdentifier);
            if (rider.IsFailure) return Result.Failure<Response>(rider.Error);

            if (!rider.Value.CnhType.Contains('A'))
                return Result.Failure<Response>(Error.Validation("RENTAL.CNH_TYPE.INVALID",
                    "CNH inválida para contratar locação"));

            logger.LogInformation("Buscando plano de aluguel de quantidade de dias: {Days}", command.RentalPlanDays);
            var rentalPlan = await rentalPlanRepository.FirstOrDefaultAsync(x => x.Days == command.RentalPlanDays);
            if (rentalPlan.IsFailure) return Result.Failure<Response>(rentalPlan.Error);

            logger.LogInformation("Criando aluguel com identificador: {Identifier}", Guid.NewGuid());
            var rental = Rental.Create(motorcycle.Value.Id, rider.Value.Id, rentalPlan.Value.Id, command.StartDate,
                command.EndDate, command.ExpectedEndDate);
            if (rental.IsFailure) return Result.Failure<Response>(rental.Error);

            await rentalRepository.AddAsync(rental.Value, cancellationToken);
            return Result.Success(new Response(rental.Value.Id));
        }
    }

    public sealed record Command(
        string RiderIdentifier,
        string MotorcycleIdentifier,
        DateOnly StartDate,
        DateOnly EndDate,
        DateOnly ExpectedEndDate,
        int RentalPlanDays
    ) : IRequest<Result<Response>>;

    public sealed record Response(Guid Id);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.RentalPlanDays)
                .GreaterThan(0).WithMessage("O plano de aluguel deve ter pelo menos um dia");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("A data de início é obrigatória")
                .Must(startDate => startDate < DateOnly.FromDateTime(DateTime.Today))
                .WithMessage("A data de início deve ser pelo menos um dia após hoje");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("A data de término é obrigatória")
                .GreaterThan(x => x.StartDate).WithMessage("A data de término deve ser posterior à data de início");

            RuleFor(x => x.ExpectedEndDate)
                .NotEmpty().WithMessage("A data prevista de término é obrigatória")
                .GreaterThan(x => x.EndDate)
                .WithMessage("A data prevista de término deve ser posterior à data de término");
        }
    }
}