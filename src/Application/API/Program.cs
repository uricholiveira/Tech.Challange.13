using API.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureApi();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Iniciando aplicação");
    var app = builder.Build();
    app.FinalizeApi();

    app.UseSerilogRequestLogging();
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Aplicação falhou ao iniciar");
}
finally
{
    Log.CloseAndFlush();
}

namespace API
{
    /// <summary>
    ///     Represents the entry point class for the application.
    ///     This class is typically used to initiate and configure the program's execution logic.
    /// </summary>
    public class Program;
}