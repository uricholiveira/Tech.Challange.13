using API.Middlewares;
using Business.Extensions;
using Infrastructure.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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

        const string serviceName = "backend-api";
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = builder.Environment.EnvironmentName,
                    ["service.version"] = "1.0.0"
                }))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation(options =>
                {
                    options.RecordException = true;
                    options.EnrichWithHttpRequest = (activity, httpRequest) =>
                    {
                        activity.SetTag("requestProtocol", httpRequest.Protocol);
                    };
                    options.EnrichWithHttpResponse = (activity, httpResponse) =>
                    {
                        activity.SetTag("responseLength", httpResponse.ContentLength);
                    };
                })
                .AddHttpClientInstrumentation(options => { options.RecordException = true; })
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri("http://localhost:4318/v1/traces");
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                }))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddProcessInstrumentation()
                .AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri("http://localhost:4318/v1/metrics");
                    options.Protocol = OtlpExportProtocol.HttpProtobuf;
                }));

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
            logging.ParseStateValues = true;

            logging.AddOtlpExporter(options =>
            {
                options.Endpoint = new Uri("http://localhost:4318/v1/logs");
                options.Protocol = OtlpExportProtocol.HttpProtobuf;
            });
        });

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