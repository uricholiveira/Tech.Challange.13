using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Domain.Extensions;

public static class DomainExtensions
{
    public static IServiceCollection ConfigureDomain(this IServiceCollection services, IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        return services;
    }
}