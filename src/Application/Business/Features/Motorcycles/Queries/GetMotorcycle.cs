using System.Text.Json.Serialization;
using Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.Helpers;

namespace Business.Features.Motorcycles.Queries;

public static class GetMotorcycle
{
    public sealed class Handler(ILogger<Handler> logger, IMotorcycleRepository motorcycleRepository)
        : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Buscando moto de identificador: {Identifier}", command.Identifier);

            var result =
                await motorcycleRepository.FirstOrDefaultAsync(x => x.Identifier == command.Identifier);
            if (result.IsFailure) return Result.Failure<Response>(result.Error);

            return Result.Success(new Response(result.Value.Identifier, result.Value.Year, result.Value.Model,
                result.Value.LicensePlate));
        }
    }

    public sealed record Command(
        string? Identifier
    ) : IRequest<Result<Response>>;

    public sealed record Response(
        [property: JsonPropertyName("identificador")]
        string Identifier,
        [property: JsonPropertyName("ano")] int Year,
        [property: JsonPropertyName("modelo")] string Model,
        [property: JsonPropertyName("placa")] string LicensePlate
    );

    public class Validator : AbstractValidator<Command>;
}