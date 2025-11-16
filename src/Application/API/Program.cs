using API.Extensions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.ConfigureApi();

try
{
    Log.Information("Iniciando aplicação");
    var app = builder.Build();
    app.FinalizeApi();
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("🚀 Aplicação iniciada!");
    logger.LogWarning("⚠️ Este é um warning de teste");

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