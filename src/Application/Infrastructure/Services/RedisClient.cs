using Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Core.Helpers;
using Shared.Core.Interfaces;
using StackExchange.Redis;

namespace Infrastructure.Services;

public class RedisClient : IRedisClient
{
    private readonly ConnectionMultiplexer _connection;
    private readonly IDatabase _database;
    private readonly ILogger<RedisClient> _logger;
    private bool _disposed;

    public RedisClient(IOptions<RedisOptions> options, ILogger<RedisClient> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        try
        {
            _logger.LogInformation("Iniciando conexão com Redis");
            _connection = ConnectionMultiplexer.Connect(options.Value.ConnectionString);
            _database = _connection.GetDatabase(0);
            _logger.LogInformation("Conexão com Redis estabelecida");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao conectar no Redis");
            throw;
        }
    }

    public bool IsConnected => _connection.IsConnected;

    public async Task<Result<bool>> SetAsync(string key, string value, TimeSpan? expiry = null)
    {
        try
        {
            _logger.LogDebug("Executando SET. Key: {Key}", key);
            var result = await _database.StringSetAsync(key, value, expiry);
            _logger.LogInformation("SET executado. Key: {Key}, Success: {Success}", key, result);
            return Result.Success(result);
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "Erro Redis ao executar SET. Key: {Key}", key);
            return Result.Failure<bool>(Error.Failure("REDIS_SET_ERROR", $"Erro ao salvar chave: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao executar SET. Key: {Key}", key);
            return Result.Failure<bool>(Error.Failure("SET_UNEXPECTED_ERROR", "Erro inesperado ao salvar"));
        }
    }

    public async Task<Result<string>> GetAsync(string key)
    {
        try
        {
            _logger.LogDebug("Executando GET. Key: {Key}", key);
            var value = await _database.StringGetAsync(key);

            if (!value.HasValue)
            {
                _logger.LogInformation("GET executado. Key: {Key}, NotFound", key);
                return Result.Failure<string>(Error.NotFound("KEY_NOT_FOUND", $"Chave '{key}' não encontrada"));
            }

            _logger.LogInformation("GET executado. Key: {Key}, Found", key);
            return Result.Success(value.ToString());
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "Erro Redis ao executar GET. Key: {Key}", key);
            return Result.Failure<string>(Error.Failure("REDIS_GET_ERROR", $"Erro ao buscar chave: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao executar GET. Key: {Key}", key);
            return Result.Failure<string>(Error.Failure("GET_UNEXPECTED_ERROR", "Erro inesperado ao buscar"));
        }
    }

    public async Task<Result<bool>> DeleteAsync(string key)
    {
        try
        {
            _logger.LogDebug("Executando DELETE. Key: {Key}", key);
            var result = await _database.KeyDeleteAsync(key);
            _logger.LogInformation("DELETE executado. Key: {Key}, Success: {Success}", key, result);
            return Result.Success(result);
        }
        catch (RedisException ex)
        {
            _logger.LogError(ex, "Erro Redis ao executar DELETE. Key: {Key}", key);
            return Result.Failure<bool>(Error.Failure("REDIS_DELETE_ERROR", $"Erro ao excluir chave: {ex.Message}"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao executar DELETE. Key: {Key}", key);
            return Result.Failure<bool>(Error.Failure("DELETE_UNEXPECTED_ERROR", "Erro inesperado ao excluir"));
        }
    }

    public async Task<Result<bool>> ExistsAsync(string key)
    {
        try
        {
            _logger.LogDebug("Verificando existência. Key: {Key}", key);
            var exists = await _database.KeyExistsAsync(key);
            _logger.LogInformation("Verificação concluída. Key: {Key}, Exists: {Exists}", key, exists);
            return Result.Success(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência. Key: {Key}", key);
            return Result.Failure<bool>(Error.Failure("EXISTS_ERROR", "Erro ao verificar existência"));
        }
    }

    public async Task<Result<bool>> SetExpiryAsync(string key, TimeSpan expiry)
    {
        try
        {
            _logger.LogDebug("Definindo expiração. Key: {Key}, Expiry: {Expiry}", key, expiry);
            var result = await _database.KeyExpireAsync(key, expiry);
            _logger.LogInformation("Expiração definida. Key: {Key}, Success: {Success}", key, result);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao definir expiração. Key: {Key}", key);
            return Result.Failure<bool>(Error.Failure("EXPIRY_ERROR", "Erro ao definir expiração"));
        }
    }

    public async Task<Result<TimeSpan?>> GetTimeToLiveAsync(string key)
    {
        try
        {
            _logger.LogDebug("Obtendo TTL. Key: {Key}", key);
            var ttl = await _database.KeyTimeToLiveAsync(key);
            _logger.LogInformation("TTL obtido. Key: {Key}, TTL: {TTL}", key, ttl);
            return Result.Success(ttl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter TTL. Key: {Key}", key);
            return Result.Failure<TimeSpan?>(Error.Failure("TTL_ERROR", "Erro ao obter TTL"));
        }
    }

    public async Task<Result<long>> IncrementAsync(string key, long value = 1)
    {
        try
        {
            _logger.LogDebug("Executando INCRBY. Key: {Key}, Value: {Value}", key, value);
            var result = await _database.StringIncrementAsync(key, value);
            _logger.LogInformation("INCRBY executado. Key: {Key}, NewValue: {NewValue}", key, result);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar INCRBY. Key: {Key}", key);
            return Result.Failure<long>(Error.Failure("INCREMENT_ERROR", "Erro ao incrementar"));
        }
    }

    public async Task<Result<long>> DecrementAsync(string key, long value = 1)
    {
        try
        {
            _logger.LogDebug("Executando DECRBY. Key: {Key}, Value: {Value}", key, value);
            var result = await _database.StringDecrementAsync(key, value);
            _logger.LogInformation("DECRBY executado. Key: {Key}, NewValue: {NewValue}", key, result);
            return Result.Success(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar DECRBY. Key: {Key}", key);
            return Result.Failure<long>(Error.Failure("DECREMENT_ERROR", "Erro ao decrementar"));
        }
    }

    public async Task<Result> PingAsync()
    {
        try
        {
            _logger.LogDebug("Executando PING");
            var server = _connection.GetServer(_connection.GetEndPoints()[0]);
            await server.PingAsync();
            _logger.LogInformation("PING executado com sucesso");
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao executar PING");
            return Result.Failure(Error.Failure("PING_ERROR", "Erro ao executar ping no Redis"));
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _logger.LogInformation("Encerrando conexão com Redis");
        GC.SuppressFinalize(this);
        _connection.Dispose();
        _disposed = true;
    }
}