using API.Middlewares;
using Business.Extensions;
using Infrastructure.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Shared.Core.Extensions;

namespace API.Extensions;

public static class HostingExtensions
{
    public static void ConfigureApi(this WebApplicationBuilder builder)
    {
        builder.Services.ConfigureBusiness(builder.Configuration, builder.Environment);

        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        builder.Services.ConfigureMediator(assemblies);
        builder.Services.ConfigureValidators();

        builder.Services.AddControllers()
            .AddJsonOptions(options =>
                JsonExtensions.ConfigureJsonOptions(options.JsonSerializerOptions, null));
        builder.Services.AddHealthChecks()
            .AddCheck("api", () => HealthCheckResult.Healthy());

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddOpenApi();
    }

    public static void FinalizeApi(this WebApplication app)
    {
        app.UseInfra();
        app.MapHealthChecks("/health");

        app.MapOpenApi();
        app.MapScalarApiReference();
        app.UseRouting();
        app.MapControllers();
        app.UseMiddleware<ExceptionMiddleware>();
    }
}