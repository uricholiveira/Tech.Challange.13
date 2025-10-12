using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Core.Helpers;
using Shared.Core.Interfaces;

namespace Infrastructure.Services;

public class RabbitMqClient : IRabbitMqClient
{
    private readonly IChannel _channel;
    private readonly IConnection _connection;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly ILogger<RabbitMqClient> _logger;
    private bool _disposed;

    private RabbitMqClient(IConnection connection, IChannel channel, ILogger<RabbitMqClient> logger)
    {
        _connection = connection;
        _channel = channel;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public bool IsConnected => _connection.IsOpen;

    public async Task<Result> PublishAsync<T>(string exchange, string routingKey, T message, bool persistent = true)
    {
        try
        {
            _logger.LogDebug("Publicando mensagem. Exchange: {Exchange}, RoutingKey: {RoutingKey}", exchange,
                routingKey);

            var json = JsonSerializer.Serialize(message, _jsonOptions);
            var body = Encoding.UTF8.GetBytes(json);

            var properties = new BasicProperties
            {
                Persistent = persistent,
                ContentType = "application/json",
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await _channel.BasicPublishAsync(exchange, routingKey, false, properties, body);

            _logger.LogInformation("Mensagem publicada. Exchange: {Exchange}, RoutingKey: {RoutingKey}", exchange,
                routingKey);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar mensagem. Exchange: {Exchange}", exchange);
            return Result.Failure(Error.Failure("PUBLISH_ERROR", $"Erro ao publicar: {ex.Message}"));
        }
    }

    public Task<Result> PublishToQueueAsync<T>(string queueName, T message, bool persistent = true)
    {
        return PublishAsync(string.Empty, queueName, message, persistent);
    }

    public async Task<Result> DeclareQueueAsync(string queueName, bool durable = true, bool exclusive = false,
        bool autoDelete = false)
    {
        try
        {
            _logger.LogDebug("Declarando fila. Queue: {Queue}", queueName);
            await _channel.QueueDeclareAsync(queueName, durable, exclusive, autoDelete);
            _logger.LogInformation("Fila declarada. Queue: {Queue}", queueName);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao declarar fila. Queue: {Queue}", queueName);
            return Result.Failure(Error.Failure("QUEUE_DECLARE_ERROR", $"Erro ao declarar fila: {ex.Message}"));
        }
    }

    public async Task<Result> DeclareExchangeAsync(string exchangeName, string type = "direct", bool durable = true,
        bool autoDelete = false)
    {
        try
        {
            _logger.LogDebug("Declarando exchange. Exchange: {Exchange}", exchangeName);
            await _channel.ExchangeDeclareAsync(exchangeName, type, durable, autoDelete);
            _logger.LogInformation("Exchange declarado. Exchange: {Exchange}", exchangeName);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao declarar exchange. Exchange: {Exchange}", exchangeName);
            return Result.Failure(Error.Failure("EXCHANGE_DECLARE_ERROR",
                $"Erro ao declarar exchange: {ex.Message}"));
        }
    }

    public async Task<Result> BindQueueAsync(string queueName, string exchangeName, string routingKey)
    {
        try
        {
            _logger.LogDebug("Vinculando fila. Queue: {Queue}, Exchange: {Exchange}", queueName, exchangeName);
            await _channel.QueueBindAsync(queueName, exchangeName, routingKey);
            _logger.LogInformation("Fila vinculada. Queue: {Queue}, Exchange: {Exchange}", queueName, exchangeName);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao vincular fila. Queue: {Queue}", queueName);
            return Result.Failure(Error.Failure("QUEUE_BIND_ERROR", $"Erro ao vincular fila: {ex.Message}"));
        }
    }

    public async Task<Result> ConsumeAsync<T>(string queueName, Func<T, Task<bool>> handler, ushort prefetchCount = 1)
    {
        try
        {
            _logger.LogInformation("Iniciando consumo. Queue: {Queue}", queueName);

            await _channel.BasicQosAsync(0, prefetchCount, false);
            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (_, ea) =>
            {
                var messageId = Guid.NewGuid().ToString();
                try
                {
                    _logger.LogDebug("Mensagem recebida. Queue: {Queue}, MessageId: {MessageId}", queueName, messageId);

                    var body = ea.Body.ToArray();
                    var json = Encoding.UTF8.GetString(body);
                    var message = JsonSerializer.Deserialize<T>(json, _jsonOptions);

                    var success = message != null && await handler(message);

                    if (success)
                    {
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                        _logger.LogInformation("Mensagem processada. Queue: {Queue}, MessageId: {MessageId}", queueName,
                            messageId);
                    }
                    else
                    {
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, true);
                        _logger.LogWarning("Mensagem rejeitada. Queue: {Queue}, MessageId: {MessageId}", queueName,
                            messageId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar mensagem. Queue: {Queue}, MessageId: {MessageId}",
                        queueName, messageId);
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            };

            await _channel.BasicConsumeAsync(queueName, false, consumer);
            _logger.LogInformation("Consumidor iniciado. Queue: {Queue}", queueName);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao iniciar consumidor. Queue: {Queue}", queueName);
            return Result.Failure(Error.Failure("CONSUME_ERROR", $"Erro ao iniciar consumidor: {ex.Message}"));
        }
    }

    public async Task<Result<uint>> GetMessageCountAsync(string queueName)
    {
        try
        {
            _logger.LogDebug("Obtendo contagem. Queue: {Queue}", queueName);
            var result = await _channel.QueueDeclarePassiveAsync(queueName);
            _logger.LogInformation("Contagem obtida. Queue: {Queue}, Count: {Count}", queueName, result.MessageCount);
            return Result.Success(result.MessageCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter contagem. Queue: {Queue}", queueName);
            return Result.Failure<uint>(Error.Failure("MESSAGE_COUNT_ERROR",
                $"Erro ao obter contagem: {ex.Message}"));
        }
    }

    public async Task<Result<uint>> PurgeQueueAsync(string queueName)
    {
        try
        {
            _logger.LogDebug("Limpando fila. Queue: {Queue}", queueName);
            var purged = await _channel.QueuePurgeAsync(queueName);
            _logger.LogInformation("Fila limpa. Queue: {Queue}, Removed: {Removed}", queueName, purged);
            return Result.Success(purged);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao limpar fila. Queue: {Queue}", queueName);
            return Result.Failure<uint>(Error.Failure("PURGE_ERROR", $"Erro ao limpar fila: {ex.Message}"));
        }
    }

    public async Task<Result<uint>> DeleteQueueAsync(string queueName)
    {
        try
        {
            _logger.LogDebug("Deletando fila. Queue: {Queue}", queueName);
            var deleted = await _channel.QueueDeleteAsync(queueName);
            _logger.LogInformation("Fila deletada. Queue: {Queue}, Deleted: {Deleted}", queueName, deleted);
            return Result.Success(deleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar fila. Queue: {Queue}", queueName);
            return Result.Failure<uint>(Error.Failure("DELETE_ERROR", $"Erro ao deletar fila: {ex.Message}"));
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;

        _logger.LogInformation("Encerrando conexão com RabbitMQ");
        GC.SuppressFinalize(this);
        try
        {
            await _channel.CloseAsync();
            await _channel.DisposeAsync();

            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao encerrar conexão");
        }

        _disposed = true;
    }

    public static async Task<Result<RabbitMqClient>> CreateAsync(string hostname, int port,
        string username, string password, string virtualHost, ILogger<RabbitMqClient> logger)
    {
        try
        {
            logger.LogInformation("Iniciando conexão com RabbitMQ");

            var factory = new ConnectionFactory
            {
                HostName = hostname,
                Port = port,
                UserName = username,
                Password = password,
                VirtualHost = virtualHost,
                AutomaticRecoveryEnabled = true
            };

            var connection = await factory.CreateConnectionAsync();
            var channel = await connection.CreateChannelAsync();

            logger.LogInformation("Conexão com RabbitMQ estabelecida");

            return Result.Success(new RabbitMqClient(connection, channel, logger));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erro ao conectar no RabbitMQ");
            return Result.Failure<RabbitMqClient>(Error.Failure("RABBITMQ_CONNECTION_ERROR",
                $"Erro ao conectar: {ex.Message}"));
        }
    }
}