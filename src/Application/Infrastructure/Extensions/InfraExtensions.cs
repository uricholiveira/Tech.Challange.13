using Amazon.S3;
using Domain.Interfaces;
using Domain.Repositories;
using Infrastructure.Data;
using Infrastructure.Options;
using Infrastructure.Repositories;
using Infrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Core.Interfaces;

namespace Infrastructure.Extensions;

public static class InfraExtensions
{
    public static IServiceCollection ConfigureInfra(this IServiceCollection services, IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.ConfigureDatabase(configuration, environment);
        services.ConfigureRepositores();

        services.ConfigureRedis(configuration);
        services.ConfigureS3(configuration);
        services.ConfigureRabbitMq(configuration);

        services.AddScoped<IImageUploadService, ImageUploadService>();
        return services;
    }

    private static void ConfigureRepositores(this IServiceCollection services)
    {
        services.AddScoped<IMotorcycleRepository, MotorcycleRepository>();
        services.AddScoped<IMotorcycleNotificationRepository, MotorcycleNotificationRepository>();
        services.AddScoped<IRentalRepository, RentalRepository>();
        services.AddScoped<IRentalPlanRepository, RentalPlanRepository>();
        services.AddScoped<IRiderRepository, RiderRepository>();
    }

    private static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentException("A string de conexão com o banco de dados está vazia/nula.");

        services.AddDbContext<DatabaseContext>(options => options
            .UseNpgsql(connectionString, config =>
            {
                config.MigrationsHistoryTable("__EFMigrationsHistory", "tech_challange");
                config.MigrationsAssembly(typeof(DatabaseContext).Assembly.GetName().Name);
            })
            .UseSnakeCaseNamingConvention()
            .EnableSensitiveDataLogging(!environment.IsProduction())
            .EnableDetailedErrors(!environment.IsProduction())
            .ConfigureWarnings(x => x.Ignore(RelationalEventId.PendingModelChangesWarning))
        );
    }

    private static void ConfigureRedis(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RedisOptions>(configuration.GetSection("Redis"));
        services.AddScoped<IRedisClient, RedisClient>();
    }

    private static void ConfigureS3(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<S3Options>(configuration.GetSection("S3"));

        services.AddScoped<IAmazonS3>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<S3Options>>().Value;

            var config = new AmazonS3Config
            {
                ServiceURL = options.ServiceUrl,
                ForcePathStyle = options.ForcePathStyle
            };

            return new AmazonS3Client(options.AccessKey, options.SecretKey, config);
        });

        services.AddScoped<IS3Client, S3Client>();
    }

    private static void ConfigureRabbitMq(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RabbitMqOptions>(configuration.GetSection("RabbitMq"));

        services.AddSingleton<IRabbitMqClient>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
            var logger = provider.GetRequiredService<ILogger<RabbitMqClient>>();

            var clientResult = RabbitMqClient.CreateAsync(
                options.Hostname,
                options.Port,
                options.Username,
                options.Password,
                options.VirtualHost,
                logger
            ).GetAwaiter().GetResult(); // Não é ideal, mas funciona no registro

            if (clientResult.IsFailure)
                throw new InvalidOperationException($"Falha ao conectar no RabbitMQ: {clientResult.Error.Message}");

            return clientResult.Value;
        });
    }

    public static void UseInfra(this IApplicationBuilder app)
    {
        app.UseMigration<DatabaseContext>();
    }
}