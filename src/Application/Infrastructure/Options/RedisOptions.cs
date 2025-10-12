namespace Infrastructure.Options;

public class RedisOptions
{
    public string ConnectionString { get; set; } = string.Empty;
    public string InstanceName { get; set; } = string.Empty;
}