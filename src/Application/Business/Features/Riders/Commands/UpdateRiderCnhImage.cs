using Domain.Interfaces;
using Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.Helpers;

namespace Business.Features.Riders.Commands;

public static class UpdateRiderCnhImage
{
    public sealed class Handler(
        ILogger<Handler> logger,
        IRiderRepository riderRepository,
        IImageUploadService imageUploadService)
        : IRequestHandler<Command, Result<Response>>
    {
        private const string BucketName = "cdn";
        private static readonly string[] AllowedFormats = ["png", "bmp"];

        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            logger.LogInformation("Buscando entregador de identificador: {Identifier}", command.Identifier);
            var existingRider = await riderRepository.FirstOrDefaultAsync(x => x.Identifier == command.Identifier);

            if (existingRider.IsFailure) return Result.Failure<Response>(existingRider.Error);

            logger.LogInformation("Fazendo upload da imagem da CNH para o S3");
            var folderPath = $"riders/{command.Identifier}";
            var cnhImageUrl = await imageUploadService.UploadBase64ImageAsync(
                BucketName,
                folderPath,
                command.CnhImage,
                AllowedFormats
            );

            if (cnhImageUrl.IsFailure)
                return Result.Failure<Response>(cnhImageUrl.Error);

            logger.LogInformation("Atualizando imagem da CNH do entregador com identificador: {Identifier}",
                command.Identifier);

            existingRider.Value.UpdateCnhImageUrl(cnhImageUrl.Value);
            await riderRepository.UpdateAsync(existingRider.Value, cancellationToken);

            logger.LogInformation("Entregador atualizado com sucesso. Id: {RiderId}", existingRider.Value.Id);
            return Result.Success(new Response());
        }
    }

    public sealed record Command(
        string Identifier,
        string CnhImage
    ) : IRequest<Result<Response>>;

    public sealed record Response;

    public class Validator : AbstractValidator<Command>;
}