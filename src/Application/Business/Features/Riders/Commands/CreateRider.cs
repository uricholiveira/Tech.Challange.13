using Domain.Entities;
using Domain.Interfaces;
using Domain.Repositories;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Shared.Core.Helpers;

namespace Business.Features.Riders.Commands;

public static class CreateRider
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
            var duplicate = await EnsureDuplicated(command);
            if (duplicate.IsFailure) return Result.Failure<Response>(duplicate.Error);

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

            logger.LogInformation("Criando entregador com identificador: {Identifier}", command.Identifier);
            var createdRider = Rider.Create(
                command.Identifier,
                command.Name,
                command.Cnpj,
                command.BirthDate,
                command.Cnh,
                command.CnhType,
                cnhImageUrl.Value);

            if (createdRider.IsFailure)
                return Result.Failure<Response>(createdRider.Error);

            await riderRepository.AddAsync(createdRider.Value, cancellationToken);

            logger.LogInformation("Entregador criado com sucesso. Id: {RiderId}", createdRider.Value.Id);
            return Result.Success(new Response(createdRider.Value.Identifier));
        }

        private async Task<Result> EnsureDuplicated(Command command)
        {
            var duplicated = await riderRepository.FirstOrDefaultAsync(x => x.Identifier == command.Identifier);
            if (duplicated.IsSuccess)
                return Result.Failure<Response>(Error.Conflict("RIDER.ALREADY_EXISTS",
                    "Já existe um entregador com este identificador"));

            var duplicatedByCnpj = await riderRepository.FirstOrDefaultAsync(x => x.Cnpj == command.Cnpj);
            if (duplicatedByCnpj.IsSuccess)
                return Result.Failure<Response>(Error.Conflict("RIDER.ALREADY_EXISTS",
                    "Já existe um entregador com este CNPJ"));

            var duplicatedByCnh = await riderRepository.FirstOrDefaultAsync(x => x.Cnh == command.Cnh);
            if (duplicatedByCnh.IsSuccess)
                return Result.Failure<Response>(Error.Conflict("RIDER.ALREADY_EXISTS",
                    "Já existe um entregador com essa CNH"));

            return Result.Success();
        }
    }

    public sealed record Command(
        string Identifier,
        string Name,
        string Cnpj,
        DateOnly BirthDate,
        string Cnh,
        string CnhType,
        string CnhImage
    ) : IRequest<Result<Response>>;

    public sealed record Response(string Identifier);

    public class Validator : AbstractValidator<Command>;
}