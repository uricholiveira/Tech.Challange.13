using Shared.Core.Helpers;

namespace Shared.Core.Interfaces;

public interface IRabbitMqClient : IAsyncDisposable
{
    bool IsConnected { get; }
    Task<Result> PublishAsync<T>(string exchange, string routingKey, T message, bool persistent = true);
    Task<Result> PublishToQueueAsync<T>(string queueName, T message, bool persistent = true);

    Task<Result> DeclareQueueAsync(string queueName, bool durable = true, bool exclusive = false,
        bool autoDelete = false);

    Task<Result> DeclareExchangeAsync(string exchangeName, string type = "direct", bool durable = true,
        bool autoDelete = false);

    Task<Result> BindQueueAsync(string queueName, string exchangeName, string routingKey);
    Task<Result> ConsumeAsync<T>(string queueName, Func<T, Task<bool>> handler, ushort prefetchCount = 1);
    Task<Result<uint>> GetMessageCountAsync(string queueName);
    Task<Result<uint>> PurgeQueueAsync(string queueName);
    Task<Result<uint>> DeleteQueueAsync(string queueName);
}