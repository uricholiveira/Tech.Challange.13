using Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.Helpers;

namespace Business.Features.Motorcycles.Commands;

public static class UpdateMotorcycleLicensePlate
{
    public sealed class Handler(ILogger<Handler> logger, IMotorcycleRepository motorcycleRepository)
        : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Buscando moto de identificador: {Identifier}", command.Identifier);

            var motorcycle =
                await motorcycleRepository.FirstOrDefaultAsync(x => x.Identifier == command.Identifier);
            if (motorcycle.IsFailure) return Result.Failure<Response>(motorcycle.Error);

            var licensePlateExists =
                await motorcycleRepository.ExistsAsync(x => x.LicensePlate == command.LicensePlate);
            if (licensePlateExists.Value)
                return Result.Failure<Response>(Error.Conflict("MOTORCYCLE.LICENSE_PLATE.ALREADY_EXISTS",
                    "Já existe uma moto com essa placa"));

            logger.LogInformation("Atualizando placa da moto de identificador: {Identifier}", command.Identifier);
            motorcycle.Value.UpdateLicensePlate(command.LicensePlate);
            await motorcycleRepository.UpdateAsync(motorcycle.Value, cancellationToken);

            logger.LogInformation("Placa atualizada com sucesso para moto de identificador: {Identifier}",
                command.Identifier);
            return Result.Success(new Response());
        }
    }

    public sealed record Command(
        string Identifier,
        string LicensePlate
    ) : IRequest<Result<Response>>;

    public sealed record Response;

    public class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.Identifier)
                .NotEmpty()
                .WithMessage("O identificador é obrigatório");

            RuleFor(x => x.LicensePlate).NotEmpty().WithMessage("A placa é obrigatória");
        }
    }
}