using System.Text.Json.Serialization;
using Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.Helpers;

namespace Business.Features.Motorcycles.Queries;

public static class ListMotorcycles
{
    public sealed class Handler(ILogger<Handler> logger, IMotorcycleRepository motorcycleRepository)
        : IRequestHandler<Command, Result<List<Response>>>
    {
        public async Task<Result<List<Response>>> Handle(Command command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Listando motos: Parâmetros(Placa={LicensePlate})", command.LicensePlate);

            var result = await motorcycleRepository.ListByParams(command.LicensePlate, null, null, cancellationToken);

            return Result.Success(result.Value
                .Select(x => new Response(x.Identifier, x.Year, x.Model, x.LicensePlate))
                .ToList()
            );
        }
    }

    public sealed record Command(
        string? LicensePlate = null
    ) : IRequest<Result<List<Response>>>;

    public sealed record Response(
        [property: JsonPropertyName("identificador")]
        string Identifier,
        [property: JsonPropertyName("ano")] int Year,
        [property: JsonPropertyName("modelo")] string Model,
        [property: JsonPropertyName("placa")] string LicensePlate
    );

    public class Validator : AbstractValidator<Command>;
}