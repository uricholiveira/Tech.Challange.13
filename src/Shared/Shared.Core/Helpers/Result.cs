namespace Shared.Core.Helpers;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        switch (isSuccess)
        {
            case true when error != Error.None:
                throw new InvalidOperationException("Sucesso não pode ter erro");
            case false when error == Error.None:
                throw new InvalidOperationException("Falha precisa ter erro");
            default:
                IsSuccess = isSuccess;
                Error = error;
                break;
        }
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    public static Result Success()
    {
        return new Result(true, Error.None);
    }

    public static Result Failure(Error error)
    {
        return new Result(false, error);
    }

    public static Result<T> Success<T>(T value)
    {
        return new Result<T>(value, true, Error.None);
    }

    public static Result<T> Failure<T>(Error error)
    {
        return new Result<T>(default!, false, error);
    }
}

public class Result<T> : Result
{
    private readonly T _value;

    internal Result(T value, bool isSuccess, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value => IsFailure
        ? throw new InvalidOperationException("Não é possível acessar o valor de um resultado com falha")
        : _value;
}

public sealed record Error
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    private Error(string code, string message, ErrorType type)
    {
        Code = code;
        Message = message;
        Type = type;
    }

    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }

    public static Error Validation(string code, string message)
    {
        return new Error(code, message, ErrorType.Validation);
    }

    public static Error NotFound(string code, string message)
    {
        return new Error(code, message, ErrorType.NotFound);
    }

    public static Error Conflict(string code, string message)
    {
        return new Error(code, message, ErrorType.Conflict);
    }

    public static Error Failure(string code, string message)
    {
        return new Error(code, message, ErrorType.Failure);
    }

    public static Error Unauthorized(string code, string message)
    {
        return new Error(code, message, ErrorType.Unauthorized);
    }

    public static Error Forbidden(string code, string message)
    {
        return new Error(code, message, ErrorType.Forbidden);
    }
}

/// <summary>
///     Tipos de erro para mapeamento HTTP
/// </summary>
public enum ErrorType
{
    None = 0,
    Validation = 1,
    NotFound = 2,
    Conflict = 3,
    Failure = 4,
    Unauthorized = 5,
    Forbidden = 6
}