using Business.Workers.Consumers;
using Business.Workers.Jobs;
using Domain.Extensions;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Business.Extensions;

public static class BusinessExtensions
{
    public static IServiceCollection ConfigureBusiness(this IServiceCollection services, IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        services.ConfigureDomain(configuration, environment);
        services.ConfigureInfra(configuration, environment);

        services.AddHostedService<EnsureRabbitMqJob>();
        services.AddHostedService<MotorcycleCreatedConsumer>();
        return services;
    }
}