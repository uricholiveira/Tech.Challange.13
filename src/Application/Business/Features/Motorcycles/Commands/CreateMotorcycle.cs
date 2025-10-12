using Domain.Entities;
using Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.Helpers;

namespace Business.Features.Motorcycles.Commands;

public static class CreateMotorcycle
{
    public sealed class Handler(ILogger<Handler> logger, IMotorcycleRepository motorcycleRepository)
        : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Buscando moto de identificador: {Identifier}", command.Identifier);

            var ensureDuplicates = await EnsureDuplicates(command);
            if (ensureDuplicates.IsFailure) return Result.Failure<Response>(ensureDuplicates.Error);

            logger.LogInformation("Criando moto com identificador: {Identifier}", command.Identifier);
            var motorcycle = Motorcycle.Create(command.Identifier, command.Year, command.Model, command.LicensePlate);
            if (motorcycle.IsFailure) return Result.Failure<Response>(motorcycle.Error);

            await motorcycleRepository.AddAsync(motorcycle.Value, cancellationToken);
            // await motorcycleRepository.SaveChangesAsync(cancellationToken);
            return Result.Success(new Response(motorcycle.Value.Identifier));
        }

        private async Task<Result> EnsureDuplicates(Command command)
        {
            var existingMotorcycle =
                await motorcycleRepository.FirstOrDefaultAsync(x => x.Identifier == command.Identifier);
            if (existingMotorcycle.IsSuccess)
                return Result.Failure<Response>(Error.Conflict("MOTORCYCLE.IDENTIFIER.ALREADY_EXISTS",
                    "Já existe uma moto com esse identificador"));

            var existingMotorcycleByLicensePlate =
                await motorcycleRepository.FirstOrDefaultAsync(x => x.LicensePlate == command.LicensePlate);
            if (existingMotorcycleByLicensePlate.IsSuccess)
                return Result.Failure<Response>(Error.Conflict("MOTORCYCLE.LICENSE_PLATE.ALREADY_EXISTS",
                    "Já existe uma moto com essa placa"));

            return Result.Success();
        }
    }

    public sealed record Command(
        string Identifier,
        int Year,
        string Model,
        string LicensePlate
    ) : IRequest<Result<Response>>;

    public sealed record Response(string Identifier);

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Identifier)
                .NotEmpty()
                .WithMessage("O identificador é obrigatório");

            RuleFor(x => x.Year)
                .NotEmpty().WithMessage("O ano é obrigatório")
                .LessThanOrEqualTo(DateTime.Now.Year)
                .WithMessage("O ano deve ser menor ou igual a {ComparisonValue}")
                .GreaterThanOrEqualTo(1900)
                .WithMessage("O ano deve ser maior ou igual a {ComparisonValue}");

            RuleFor(x => x.Model).NotEmpty().WithMessage("O modelo é obrigatório");
            RuleFor(x => x.LicensePlate).NotEmpty().WithMessage("A placa é obrigatória");
        }
    }
}