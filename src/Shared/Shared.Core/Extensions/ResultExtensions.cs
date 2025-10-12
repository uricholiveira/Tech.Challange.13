using Microsoft.AspNetCore.Mvc;
using Shared.Core.Helpers;

namespace Shared.Core.Extensions;

public static class ResultExtensions
{
    /// <summary>
    ///     Executa uma ação se o resultado for sucesso
    /// </summary>
    public static Result OnSuccess(this Result result, Action action)
    {
        if (result.IsSuccess)
            action();

        return result;
    }

    /// <summary>
    ///     Executa uma ação se o resultado for sucesso (com valor)
    /// </summary>
    public static Result<T> OnSuccess<T>(this Result<T> result, Action<T> action)
    {
        if (result.IsSuccess)
            action(result.Value);

        return result;
    }

    /// <summary>
    ///     Executa uma ação se o resultado for falha
    /// </summary>
    public static Result OnFailure(this Result result, Action<Error> action)
    {
        if (result.IsFailure)
            action(result.Error);

        return result;
    }

    /// <summary>
    ///     Executa uma ação se o resultado for falha (com valor)
    /// </summary>
    public static Result<T> OnFailure<T>(this Result<T> result, Action<Error> action)
    {
        if (result.IsFailure)
            action(result.Error);

        return result;
    }

    /// <summary>
    ///     Transforma o valor de um resultado com sucesso
    /// </summary>
    public static Result<TOut> Map<TIn, TOut>(this Result<TIn> result, Func<TIn, TOut> mapper)
    {
        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error);

        return Result.Success(mapper(result.Value));
    }

    /// <summary>
    ///     Encadeia operações que retornam Result
    /// </summary>
    public static Result<TOut> Bind<TIn, TOut>(this Result<TIn> result, Func<TIn, Result<TOut>> binder)
    {
        if (result.IsFailure)
            return Result.Failure<TOut>(result.Error);

        return binder(result.Value);
    }

    /// <summary>
    ///     Retorna o valor ou um valor padrão se for falha
    /// </summary>
    public static T? GetValueOrDefault<T>(this Result<T> result, T? defaultValue = default)
    {
        return result.IsSuccess ? result.Value : defaultValue;
    }

    /// <summary>
    ///     Combina múltiplos resultados em um único
    /// </summary>
    public static Result Combine(params Result[] results)
    {
        var failures = results.Where(r => r.IsFailure).ToList();

        if (failures.Count == 0)
            return Result.Success();

        var errors = string.Join(", ", failures.Select(f => f.Error.Message));
        return Result.Failure(Error.Failure("COMBINED_ERRORS", errors));
    }

    /// <summary>
    ///     Valida uma condição e retorna Result
    /// </summary>
    public static Result Ensure(this Result result, Func<bool> predicate, Error error)
    {
        if (result.IsFailure)
            return result;

        return predicate() ? Result.Success() : Result.Failure(error);
    }

    /// <summary>
    ///     Valida uma condição e retorna Result com valor
    /// </summary>
    public static Result<T> Ensure<T>(this Result<T> result, Func<T, bool> predicate, Error error)
    {
        if (result.IsFailure)
            return result;

        return predicate(result.Value) ? result : Result.Failure<T>(error);
    }
}

/// <summary>
///     Métodos de extensão para converter Result em respostas HTTP
/// </summary>
public static class ResultHttpExtensions
{
    /// <summary>
    ///     Converte um Result em IActionResult
    /// </summary>
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
            return new OkResult();

        return result.Error.Type switch
        {
            ErrorType.Validation => new BadRequestObjectResult(new ProblemDetails
            {
                Status = 400,
                Title = "Erro de validação",
                Detail = result.Error.Message,
                Type = result.Error.Code
            }),
            ErrorType.NotFound => new NotFoundObjectResult(new ProblemDetails
            {
                Status = 404,
                Title = "Não encontrado",
                Detail = result.Error.Message,
                Type = result.Error.Code
            }),
            ErrorType.Conflict => new ConflictObjectResult(new ProblemDetails
            {
                Status = 409,
                Title = "Conflito",
                Detail = result.Error.Message,
                Type = result.Error.Code
            }),
            ErrorType.Unauthorized => new UnauthorizedObjectResult(new ProblemDetails
            {
                Status = 401,
                Title = "Não autorizado",
                Detail = result.Error.Message,
                Type = result.Error.Code
            }),
            ErrorType.Forbidden => new ObjectResult(new ProblemDetails
            {
                Status = 403,
                Title = "Acesso negado",
                Detail = result.Error.Message,
                Type = result.Error.Code
            })
            {
                StatusCode = 403
            },
            _ => new ObjectResult(new ProblemDetails
            {
                Status = 500,
                Title = "Erro interno do servidor",
                Detail = result.Error.Message,
                Type = result.Error.Code
            })
            {
                StatusCode = 500
            }
        };
    }

    /// <summary>
    ///     Converte um Result com valor em IActionResult
    /// </summary>
    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
            return new OkObjectResult(result.Value);

        return ((Result)result).ToActionResult();
    }

    /// <summary>
    ///     Converte um Result em IActionResult com código 201 (Created)
    /// </summary>
    public static IActionResult ToCreatedResult<T>(this Result<T> result, string location)
    {
        if (result.IsSuccess)
            return new CreatedResult(location, result.Value);

        return ((Result)result).ToActionResult();
    }

    /// <summary>
    ///     Converte um Result em IActionResult com código 201 (Created) usando route
    /// </summary>
    public static IActionResult ToCreatedAtRouteResult<T>(this Result<T> result, string routeName, object routeValues)
    {
        if (result.IsSuccess)
            return new CreatedAtRouteResult(routeName, routeValues, result.Value);

        return ((Result)result).ToActionResult();
    }

    /// <summary>
    ///     Converte um Result em IActionResult com código 204 (No Content)
    /// </summary>
    public static IActionResult ToNoContentResult(this Result result)
    {
        if (result.IsSuccess)
            return new NoContentResult();

        return result.ToActionResult();
    }

    /// <summary>
    ///     Converte Result em resposta customizada de erro
    /// </summary>
    public static IActionResult ToCustomErrorResult(this Result result,
        Dictionary<string, object>? additionalData = null)
    {
        if (result.IsSuccess)
            return new OkResult();

        var errorResponse = new
        {
            error = new
            {
                code = result.Error.Code,
                message = result.Error.Message,
                type = result.Error.Type.ToString(),
                timestamp = DateTime.UtcNow,
                additionalData
            }
        };

        var statusCode = result.Error.Type switch
        {
            ErrorType.Validation => 400,
            ErrorType.NotFound => 404,
            ErrorType.Conflict => 409,
            ErrorType.Unauthorized => 401,
            ErrorType.Forbidden => 403,
            _ => 500
        };

        return new ObjectResult(errorResponse) { StatusCode = statusCode };
    }
}