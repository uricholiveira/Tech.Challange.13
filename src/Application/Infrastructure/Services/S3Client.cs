using System.Net;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Logging;
using Shared.Core.Helpers;
using Shared.Core.Interfaces;
using Shared.Core.Models.Common;

namespace Infrastructure.Services;

public class S3Client : IS3Client
{
    private readonly ILogger<S3Client> _logger;
    private readonly IAmazonS3 _s3Client;
    private bool _disposed;

    public S3Client(IAmazonS3 s3Client, ILogger<S3Client> logger)
    {
        _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _logger.LogInformation("S3 Client inicializado com sucesso");
    }

    public Task<Result<string>> GetPublicUrlAsync(string bucketName, string key)
    {
        try
        {
            _logger.LogDebug("Gerando URL pública. Bucket: {Bucket}, Key: {Key}", bucketName, key);

            var url = $"{_s3Client.Config.ServiceURL}/{bucketName}/{key}";

            _logger.LogInformation("URL pública gerada. Bucket: {Bucket}, Key: {Key}, URL: {Url}",
                bucketName, key, url);

            return Task.FromResult(Result.Success(url));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao gerar URL pública. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Task.FromResult(Result.Failure<string>(Error.Failure("PUBLIC_URL_ERROR",
                "Erro ao gerar URL pública")));
        }
    }

    #region URLs Pré-assinadas

    public Task<Result<string>> GetPresignedUrlAsync(string bucketName, string key, int expiryInMinutes = 60)
    {
        try
        {
            _logger.LogDebug("Gerando URL pré-assinada. Bucket: {Bucket}, Key: {Key}, Expiry: {Expiry}min",
                bucketName, key, expiryInMinutes);

            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Expires = DateTime.UtcNow.AddMinutes(expiryInMinutes)
            };

            var url = _s3Client.GetPreSignedURL(request);

            _logger.LogInformation(
                "URL pré-assinada gerada com sucesso. Bucket: {Bucket}, Key: {Key}, Expiry: {Expiry}min",
                bucketName, key, expiryInMinutes);

            return Task.FromResult(Result.Success(url));
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro S3 ao gerar URL pré-assinada. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Task.FromResult(
                Result.Failure<string>(Error.Failure("PRESIGNED_URL_ERROR", $"Erro ao gerar URL: {ex.Message}")));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao gerar URL pré-assinada. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Task.FromResult(Result.Failure<string>(Error.Failure("PRESIGNED_URL_UNEXPECTED_ERROR",
                "Erro inesperado ao gerar URL")));
        }
    }

    #endregion


    public async Task<Result> DeleteObjectsAsync(string bucketName, IEnumerable<string> keys)
    {
        try
        {
            var keysList = keys.ToList();

            _logger.LogDebug("Deletando múltiplos objetos. Bucket: {Bucket}, Count: {Count}",
                bucketName, keysList.Count);

            var request = new DeleteObjectsRequest
            {
                BucketName = bucketName,
                Objects = keysList.Select(k => new KeyVersion { Key = k }).ToList()
            };

            var response = await _s3Client.DeleteObjectsAsync(request);

            if (response.DeleteErrors.Count != 0)
            {
                var errors = string.Join(", ", response.DeleteErrors.Select(e => $"{e.Key}: {e.Message}"));

                _logger.LogWarning(
                    "Deleção em lote concluída com erros. Bucket: {Bucket}, Total: {Total}, Errors: {Errors}",
                    bucketName, keysList.Count, response.DeleteErrors.Count);

                return Result.Failure(Error.Failure("DELETE_OBJECTS_PARTIAL_ERROR",
                    $"Alguns objetos não foram deletados. Erros: {errors}"));
            }

            _logger.LogInformation("Deleção em lote concluída com sucesso. Bucket: {Bucket}, Count: {Count}",
                bucketName, keysList.Count);

            return Result.Success();
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro S3 ao deletar objetos em lote. Bucket: {Bucket}", bucketName);
            return Result.Failure(Error.Failure("DELETE_OBJECTS_ERROR", $"Erro ao deletar objetos: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao deletar objetos em lote. Bucket: {Bucket}", bucketName);
            return Result.Failure(
                Error.Failure("DELETE_OBJECTS_UNEXPECTED_ERROR", "Erro inesperado ao deletar objetos"));
        }
    }

    #region Helpers

    private static string GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".txt" => "text/plain",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".svg" => "image/svg+xml",
            ".webp" => "image/webp",
            ".csv" => "text/csv",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".html" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            ".7z" => "application/x-7z-compressed",
            ".tar" => "application/x-tar",
            ".gz" => "application/gzip",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".ogg" => "audio/ogg",
            ".mp4" => "video/mp4",
            ".avi" => "video/x-msvideo",
            ".mov" => "video/quicktime",
            ".wmv" => "video/x-ms-wmv",
            _ => "application/octet-stream"
        };
    }

    #endregion

    public void Dispose()
    {
        Dispose(true);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            _logger.LogInformation("Encerrando S3 Client");
            _s3Client.Dispose();
        }

        _disposed = true;
    }

    #region Operações de Bucket

    public async Task<Result<bool>> BucketExistsAsync(string bucketName)
    {
        try
        {
            _logger.LogDebug("Verificando existência de bucket. Bucket: {Bucket}", bucketName);

            var exists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);

            _logger.LogInformation("Verificação de bucket concluída. Bucket: {Bucket}, Exists: {Exists}",
                bucketName, exists);

            return Result.Success(exists);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro S3 ao verificar bucket. Bucket: {Bucket}", bucketName);
            return Result.Failure<bool>(Error.Failure("BUCKET_EXISTS_ERROR",
                $"Erro ao verificar bucket: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao verificar bucket. Bucket: {Bucket}", bucketName);
            return Result.Failure<bool>(Error.Failure("BUCKET_EXISTS_UNEXPECTED_ERROR",
                "Erro inesperado ao verificar bucket"));
        }
    }

    public async Task<Result> CreateBucketAsync(string bucketName)
    {
        try
        {
            _logger.LogDebug("Criando bucket. Bucket: {Bucket}", bucketName);

            var existsResult = await BucketExistsAsync(bucketName);
            if (existsResult.IsFailure)
                return Result.Failure(existsResult.Error);

            if (existsResult.Value)
            {
                _logger.LogInformation("Bucket já existe. Bucket: {Bucket}", bucketName);
                return Result.Success();
            }

            var request = new PutBucketRequest
            {
                BucketName = bucketName,
                UseClientRegion = true
            };

            await _s3Client.PutBucketAsync(request);

            _logger.LogInformation("Bucket criado com sucesso. Bucket: {Bucket}", bucketName);
            return Result.Success();
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro S3 ao criar bucket. Bucket: {Bucket}", bucketName);
            return Result.Failure(Error.Failure("BUCKET_CREATE_ERROR", $"Erro ao criar bucket: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao criar bucket. Bucket: {Bucket}", bucketName);
            return Result.Failure(Error.Failure("BUCKET_CREATE_UNEXPECTED_ERROR", "Erro inesperado ao criar bucket"));
        }
    }

    public async Task<Result> DeleteBucketAsync(string bucketName)
    {
        try
        {
            _logger.LogDebug("Deletando bucket. Bucket: {Bucket}", bucketName);

            await _s3Client.DeleteBucketAsync(bucketName);

            _logger.LogInformation("Bucket deletado com sucesso. Bucket: {Bucket}", bucketName);
            return Result.Success();
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro S3 ao deletar bucket. Bucket: {Bucket}", bucketName);
            return Result.Failure(Error.Failure("BUCKET_DELETE_ERROR", $"Erro ao deletar bucket: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao deletar bucket. Bucket: {Bucket}", bucketName);
            return Result.Failure(Error.Failure("BUCKET_DELETE_UNEXPECTED_ERROR", "Erro inesperado ao deletar bucket"));
        }
    }

    public async Task<Result<List<string>>> ListBucketsAsync()
    {
        try
        {
            _logger.LogDebug("Listando buckets");

            var response = await _s3Client.ListBucketsAsync();
            var bucketNames = response.Buckets.Select(b => b.BucketName).ToList();

            _logger.LogInformation("Buckets listados com sucesso. Count: {Count}", bucketNames.Count);

            return Result.Success(bucketNames);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro S3 ao listar buckets");
            return Result.Failure<List<string>>(Error.Failure("LIST_BUCKETS_ERROR",
                $"Erro ao listar buckets: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao listar buckets");
            return Result.Failure<List<string>>(Error.Failure("LIST_BUCKETS_UNEXPECTED_ERROR",
                "Erro inesperado ao listar buckets"));
        }
    }

    #endregion

    #region Upload de Objetos

    public async Task<Result<string>> UploadFileAsync(string bucketName, string key, string filePath,
        string? contentType = null)
    {
        try
        {
            _logger.LogDebug("Fazendo upload de arquivo. Bucket: {Bucket}, Key: {Key}, FilePath: {FilePath}",
                bucketName, key, filePath);

            if (!File.Exists(filePath))
                return Result.Failure<string>(Error.NotFound("FILE_NOT_FOUND", $"Arquivo não encontrado: {filePath}"));

            var fileInfo = new FileInfo(filePath);
            contentType ??= GetContentType(filePath);

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                FilePath = filePath,
                ContentType = contentType
            };

            var response = await _s3Client.PutObjectAsync(request);

            _logger.LogInformation(
                "Upload de arquivo concluído com sucesso. Bucket: {Bucket}, Key: {Key}, Size: {Size} bytes, ETag: {ETag}",
                bucketName, key, fileInfo.Length, response.ETag);

            return Result.Success(response.ETag);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro S3 ao fazer upload de arquivo. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Result.Failure<string>(Error.Failure("UPLOAD_FILE_ERROR", $"Erro ao fazer upload: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao fazer upload de arquivo. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Result.Failure<string>(Error.Failure("UPLOAD_FILE_UNEXPECTED_ERROR",
                "Erro inesperado ao fazer upload"));
        }
    }

    public async Task<Result<string>> UploadStreamAsync(string bucketName, string key, Stream stream,
        string? contentType = null)
    {
        try
        {
            _logger.LogDebug("Fazendo upload de stream. Bucket: {Bucket}, Key: {Key}, Size: {Size}",
                bucketName, key, stream.Length);

            contentType ??= "application/octet-stream";

            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = stream,
                ContentType = contentType
            };

            var response = await _s3Client.PutObjectAsync(request);

            _logger.LogInformation("Upload de stream concluído com sucesso. Bucket: {Bucket}, Key: {Key}, ETag: {ETag}",
                bucketName, key, response.ETag);

            return Result.Success(response.ETag);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro S3 ao fazer upload de stream. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Result.Failure<string>(Error.Failure("UPLOAD_STREAM_ERROR", $"Erro ao fazer upload: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao fazer upload de stream. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Result.Failure<string>(Error.Failure("UPLOAD_STREAM_UNEXPECTED_ERROR",
                "Erro inesperado ao fazer upload"));
        }
    }

    public async Task<Result<string>> UploadBytesAsync(string bucketName, string key, byte[] data,
        string? contentType = null)
    {
        try
        {
            _logger.LogDebug("Fazendo upload de bytes. Bucket: {Bucket}, Key: {Key}, Size: {Size}",
                bucketName, key, data.Length);

            using var stream = new MemoryStream(data);
            return await UploadStreamAsync(bucketName, key, stream, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload de bytes. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Result.Failure<string>(Error.Failure("UPLOAD_BYTES_ERROR", $"Erro ao fazer upload: {ex.Message}"));
        }
    }

    #endregion

    #region Download de Objetos

    public async Task<Result<Stream>> DownloadStreamAsync(string bucketName, string key)
    {
        try
        {
            _logger.LogDebug("Fazendo download de stream. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);

            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(request);
            var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            _logger.LogInformation("Download de stream concluído. Bucket: {Bucket}, Key: {Key}, Size: {Size} bytes",
                bucketName, key, memoryStream.Length);

            return Result.Success<Stream>(memoryStream);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Objeto não encontrado. Bucket: {Bucket}, Key: {Key}", bucketName, key);
            return Result.Failure<Stream>(Error.NotFound("OBJECT_NOT_FOUND", $"Objeto '{key}' não encontrado"));
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro S3 ao fazer download de stream. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Result.Failure<Stream>(Error.Failure("DOWNLOAD_STREAM_ERROR",
                $"Erro ao fazer download: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao fazer download de stream. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Result.Failure<Stream>(Error.Failure("DOWNLOAD_STREAM_UNEXPECTED_ERROR",
                "Erro inesperado ao fazer download"));
        }
    }

    public async Task<Result<byte[]>> DownloadBytesAsync(string bucketName, string key)
    {
        try
        {
            _logger.LogDebug("Fazendo download de bytes. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);

            var streamResult = await DownloadStreamAsync(bucketName, key);

            if (streamResult.IsFailure)
                return Result.Failure<byte[]>(streamResult.Error);

            await using var stream = streamResult.Value;
            var bytes = ((MemoryStream)stream).ToArray();

            return Result.Success(bytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer download de bytes. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Result.Failure<byte[]>(
                Error.Failure("DOWNLOAD_BYTES_ERROR", $"Erro ao fazer download: {ex.Message}"));
        }
    }

    public async Task<Result> DownloadFileAsync(string bucketName, string key, string filePath)
    {
        try
        {
            _logger.LogDebug("Fazendo download de arquivo. Bucket: {Bucket}, Key: {Key}, FilePath: {FilePath}",
                bucketName, key, filePath);

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            using var response = await _s3Client.GetObjectAsync(request);
            await response.WriteResponseStreamToFileAsync(filePath, false, CancellationToken.None);

            var fileInfo = new FileInfo(filePath);

            _logger.LogInformation(
                "Download de arquivo concluído. Bucket: {Bucket}, Key: {Key}, FilePath: {FilePath}, Size: {Size} bytes",
                bucketName, key, filePath, fileInfo.Length);

            return Result.Success();
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Objeto não encontrado. Bucket: {Bucket}, Key: {Key}", bucketName, key);
            return Result.Failure(Error.NotFound("OBJECT_NOT_FOUND", $"Objeto '{key}' não encontrado"));
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro S3 ao fazer download de arquivo. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Result.Failure(Error.Failure("DOWNLOAD_FILE_ERROR", $"Erro ao fazer download: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao fazer download de arquivo. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Result.Failure(Error.Failure("DOWNLOAD_FILE_UNEXPECTED_ERROR", "Erro inesperado ao fazer download"));
        }
    }

    #endregion

    #region Gerenciamento de Objetos

    public async Task<Result<bool>> ObjectExistsAsync(string bucketName, string key)
    {
        try
        {
            _logger.LogDebug("Verificando existência de objeto. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);

            var request = new GetObjectMetadataRequest
            {
                BucketName = bucketName,
                Key = key
            };

            await _s3Client.GetObjectMetadataAsync(request);

            _logger.LogInformation("Objeto existe. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);

            return Result.Success(true);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogInformation("Objeto não existe. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Result.Success(false);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro S3 ao verificar objeto. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Result.Failure<bool>(Error.Failure("OBJECT_EXISTS_ERROR",
                $"Erro ao verificar objeto: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao verificar objeto. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Result.Failure<bool>(Error.Failure("OBJECT_EXISTS_UNEXPECTED_ERROR",
                "Erro inesperado ao verificar objeto"));
        }
    }

    public async Task<Result> DeleteObjectAsync(string bucketName, string key)
    {
        try
        {
            _logger.LogDebug("Deletando objeto. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);

            await _s3Client.DeleteObjectAsync(bucketName, key);

            _logger.LogInformation("Objeto deletado com sucesso. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);

            return Result.Success();
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro S3 ao deletar objeto. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Result.Failure(Error.Failure("DELETE_OBJECT_ERROR", $"Erro ao deletar objeto: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao deletar objeto. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Result.Failure(Error.Failure("DELETE_OBJECT_UNEXPECTED_ERROR", "Erro inesperado ao deletar objeto"));
        }
    }

    public async Task<Result<List<string>>> ListObjectsAsync(string bucketName, string? prefix = null)
    {
        try
        {
            _logger.LogDebug("Listando objetos. Bucket: {Bucket}, Prefix: {Prefix}",
                bucketName, prefix ?? "null");

            var objects = new List<string>();
            var request = new ListObjectsV2Request
            {
                BucketName = bucketName,
                Prefix = prefix
            };

            ListObjectsV2Response response;
            do
            {
                response = await _s3Client.ListObjectsV2Async(request);
                objects.AddRange(response.S3Objects.Select(o => o.Key));
                request.ContinuationToken = response.NextContinuationToken;
            } while ((bool)response.IsTruncated!);

            _logger.LogInformation("Objetos listados com sucesso. Bucket: {Bucket}, Prefix: {Prefix}, Count: {Count}",
                bucketName, prefix ?? "null", objects.Count);

            return Result.Success(objects);
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro S3 ao listar objetos. Bucket: {Bucket}, Prefix: {Prefix}",
                bucketName, prefix ?? "null");
            return Result.Failure<List<string>>(Error.Failure("LIST_OBJECTS_ERROR",
                $"Erro ao listar objetos: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao listar objetos. Bucket: {Bucket}, Prefix: {Prefix}",
                bucketName, prefix);
            return Result.Failure<List<string>>(Error.Failure("LIST_OBJECTS_UNEXPECTED_ERROR",
                "Erro inesperado ao listar objetos"));
        }
    }

    public async Task<Result<ObjectMetadata>> GetObjectMetadataAsync(string bucketName, string key)
    {
        try
        {
            _logger.LogDebug("Obtendo metadata de objeto. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);

            var request = new GetObjectMetadataRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectMetadataAsync(request);

            var metadata = new ObjectMetadata
            {
                Key = key,
                Size = response.ContentLength,
                LastModified = response.LastModified,
                ETag = response.ETag,
                ContentType = response.Headers.ContentType,
                Metadata = response.Metadata.Keys
                    .ToDictionary(k => k, k => response.Metadata[k])
            };

            _logger.LogInformation("Metadata obtida com sucesso. Bucket: {Bucket}, Key: {Key}, Size: {Size} bytes",
                bucketName, key, metadata.Size);

            return Result.Success(metadata);
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Objeto não encontrado. Bucket: {Bucket}, Key: {Key}", bucketName, key);
            return Result.Failure<ObjectMetadata>(Error.NotFound("OBJECT_NOT_FOUND", $"Objeto '{key}' não encontrado"));
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro S3 ao obter metadata. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Result.Failure<ObjectMetadata>(Error.Failure("GET_METADATA_ERROR",
                $"Erro ao obter metadata: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao obter metadata. Bucket: {Bucket}, Key: {Key}",
                bucketName, key);
            return Result.Failure<ObjectMetadata>(Error.Failure("GET_METADATA_UNEXPECTED_ERROR",
                "Erro inesperado ao obter metadata"));
        }
    }

    #endregion
}