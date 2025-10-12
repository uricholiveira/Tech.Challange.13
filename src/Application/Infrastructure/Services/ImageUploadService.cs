using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using MimeDetective;
using MimeDetective.Definitions;
using Shared.Core.Helpers;
using Shared.Core.Interfaces;

namespace Infrastructure.Services;

public class ImageUploadService(IS3Client s3Client, ILogger<ImageUploadService> logger) : IImageUploadService
{
    public async Task<Result<string>> UploadBase64ImageAsync(
        string bucketName,
        string folderPath,
        string base64Image,
        string[] allowedFormats)
    {
        try
        {
            // Remove o prefixo data:image/...;base64, se existir
            var base64Data = base64Image.Contains(',')
                ? base64Image.Split(',')[1]
                : base64Image;

            var imageBytes = Convert.FromBase64String(base64Data);

            var inspector = new ContentInspectorBuilder
            {
                Definitions = DefaultDefinitions.All()
            }.Build();

            var results = inspector.Inspect(imageBytes);

            if (!results.Any())
                return Result.Failure<string>(Error.Validation("IMAGE.INVALID_FORMAT",
                    "Não foi possível detectar o formato da imagem"));

            var detectedMime = results.First().Definition.File.MimeType;
            var detectedExtension = results.First().Definition.File.Extensions.FirstOrDefault()?.ToLower();

            if (detectedExtension == null || allowedFormats.All(f =>
                    !f.Equals(detectedExtension, StringComparison.CurrentCultureIgnoreCase)))
            {
                var allowedList = string.Join(", ", allowedFormats.Select(f => f.ToUpper()));
                return Result.Failure<string>(Error.Validation("IMAGE.INVALID_TYPE",
                    $"Formato de imagem deve ser: {allowedList}"));
            }

            var key = $"{folderPath}/{Guid.NewGuid()}.{detectedExtension}";

            logger.LogInformation("Fazendo upload da imagem para: {Key}", key);

            var uploadResult = await s3Client.UploadBytesAsync(
                bucketName,
                key,
                imageBytes,
                detectedMime);

            if (uploadResult.IsFailure)
                return Result.Failure<string>(uploadResult.Error);

            var url = await s3Client.GetPublicUrlAsync(bucketName, key);
            if (url.IsFailure) return Result.Failure<string>(url.Error);

            logger.LogInformation("Upload concluído com sucesso: {Url}", url.Value);
            return Result.Success(url.Value);
        }
        catch (FormatException)
        {
            logger.LogError("Base64 inválido fornecido");
            return Result.Failure<string>(Error.Validation("IMAGE.INVALID_BASE64",
                "Formato base64 inválido"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao fazer upload da imagem");
            return Result.Failure<string>(Error.Failure("IMAGE.UPLOAD_FAILED",
                "Falha ao fazer upload da imagem"));
        }
    }
}