using Shared.Core.Helpers;

namespace Shared.Core.Interfaces;

public interface IRedisClient : IDisposable
{
    bool IsConnected { get; }
    Task<Result<bool>> SetAsync(string key, string value, TimeSpan? expiry = null);
    Task<Result<string>> GetAsync(string key);
    Task<Result<bool>> DeleteAsync(string key);
    Task<Result<bool>> ExistsAsync(string key);
    Task<Result<bool>> SetExpiryAsync(string key, TimeSpan expiry);
    Task<Result<TimeSpan?>> GetTimeToLiveAsync(string key);
    Task<Result<long>> IncrementAsync(string key, long value = 1);
    Task<Result<long>> DecrementAsync(string key, long value = 1);
    Task<Result> PingAsync();
}