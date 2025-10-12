using System.Text.Json.Serialization;
using Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.Helpers;

namespace Business.Features.Riders.Queries;

public static class GetRider
{
    public sealed class Handler(ILogger<Handler> logger, IRiderRepository riderRepository)
        : IRequestHandler<Command, Result<Response>>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Buscando moto de identificador: {Identifier}", command.Identifier);

            var result =
                await riderRepository.FirstOrDefaultAsync(x => x.Identifier == command.Identifier);
            if (result.IsFailure) return Result.Failure<Response>(result.Error);

            return Result.Success(new Response(result.Value.Identifier, result.Value.Name, result.Value.Cnpj,
                result.Value.BirthDate, result.Value.Cnh, result.Value.CnhType, result.Value.CnhImageUrl)
            );
        }
    }

    public sealed record Command(
        string? Identifier
    ) : IRequest<Result<Response>>;

    public sealed record Response(
        [property: JsonPropertyName("identificador")]
        string Identifier,
        [property: JsonPropertyName("nome")] string Name,
        [property: JsonPropertyName("cnpj")] string Cnpj,
        [property: JsonPropertyName("data_nascimento")]
        DateOnly BirthDate,
        [property: JsonPropertyName("cnh")] string Cnh,
        [property: JsonPropertyName("tipo_cnh")]
        string CnhType,
        [property: JsonPropertyName("imagem_cnh")]
        string CnhImageUrl
    );

    public class Validator : AbstractValidator<Command>;
}