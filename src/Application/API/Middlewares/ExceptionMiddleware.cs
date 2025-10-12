using System.Text.Json;
using FluentValidation;

namespace API.Middlewares;

public class ExceptionMiddleware(
    RequestDelegate next,
    ILogger<ExceptionMiddleware> logger)
{
    private readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/json";

            var json = JsonSerializer.Serialize(
                new
                {
                    Error = new
                    {
                        Code = "Common.Validation",
                        Message = "Erro na validação dos dados",
                        Details = ex.Errors.Select(e => new { Property = e.PropertyName, Error = e.ErrorMessage })
                    }
                },
                _options
            );
            await context.Response.WriteAsync(json);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            logger.LogError(
                ex,
                "Ocorreu um erro inesperado durante o processamento da solicitação."
            );

            var json = JsonSerializer.Serialize(
                new
                {
                    Error = new
                    {
                        Code = "Common.InternalServer",
                        Message = "Erro interno no servidor"
                    }
                },
                _options
            );
            await context.Response.WriteAsync(json);
        }
    }
}