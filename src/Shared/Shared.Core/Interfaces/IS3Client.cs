using Shared.Core.Helpers;
using Shared.Core.Models.Common;

namespace Shared.Core.Interfaces;

public interface IS3Client
{
    Task<Result<bool>> BucketExistsAsync(string bucketName);
    Task<Result> CreateBucketAsync(string bucketName);
    Task<Result> DeleteBucketAsync(string bucketName);
    Task<Result<List<string>>> ListBucketsAsync();
    Task<Result<string>> UploadFileAsync(string bucketName, string key, string filePath, string? contentType = null);
    Task<Result<string>> UploadStreamAsync(string bucketName, string key, Stream stream, string? contentType = null);
    Task<Result<string>> UploadBytesAsync(string bucketName, string key, byte[] data, string? contentType = null);
    Task<Result<Stream>> DownloadStreamAsync(string bucketName, string key);
    Task<Result<byte[]>> DownloadBytesAsync(string bucketName, string key);
    Task<Result> DownloadFileAsync(string bucketName, string key, string filePath);
    Task<Result<bool>> ObjectExistsAsync(string bucketName, string key);
    Task<Result> DeleteObjectAsync(string bucketName, string key);
    Task<Result<List<string>>> ListObjectsAsync(string bucketName, string? prefix = null);
    Task<Result<ObjectMetadata>> GetObjectMetadataAsync(string bucketName, string key);
    Task<Result<string>> GetPublicUrlAsync(string bucketName, string key);
    Task<Result<string>> GetPresignedUrlAsync(string bucketName, string key, int expiryInMinutes = 60);
    Task<Result> DeleteObjectsAsync(string bucketName, IEnumerable<string> keys);
}