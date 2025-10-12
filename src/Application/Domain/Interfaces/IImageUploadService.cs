using Shared.Core.Helpers;

namespace Domain.Interfaces;

public interface IImageUploadService
{
    Task<Result<string>> UploadBase64ImageAsync(
        string bucketName,
        string folderPath,
        string base64Image,
        string[] allowedFormats);
}