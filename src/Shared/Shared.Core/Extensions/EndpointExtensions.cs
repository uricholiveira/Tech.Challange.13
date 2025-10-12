using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Shared.Core.Interfaces;

namespace Shared.Core.Extensions;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(this IServiceCollection services, Assembly assembly)
    {
        var endpointTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                        t.GetInterfaces().Any(i => i == typeof(IEndpoint)))
            .ToList();

        return services;
    }

    public static WebApplication MapEndpoints(this WebApplication app, Assembly? assembly = null)
    {
        assembly ??= Assembly.GetCallingAssembly();

        var endpointTypes = assembly.GetTypes()
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                        t.GetInterfaces().Any(i => i == typeof(IEndpoint)))
            .ToList();

        foreach (var method in endpointTypes.Select(type => type.GetMethod(nameof(IEndpoint.MapEndpoint),
                     BindingFlags.Public | BindingFlags.Static)))
            method?.Invoke(null, [app]);

        return app;
    }

    public static WebApplication MapEndpoints(this WebApplication app, params Assembly[] assemblies)
    {
        foreach (var assembly in assemblies) app.MapEndpoints(assembly);

        return app;
    }
}