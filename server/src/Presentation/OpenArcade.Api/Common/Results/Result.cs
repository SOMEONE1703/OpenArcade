namespace OpenArcade.Api.Common.Results;

public enum ErrorKind { NotFound, Forbidden, Conflict, Invalid }

public sealed record Error(ErrorKind Kind, string? Message = null)
{
    public static Error NotFound(string? message = null) => new(ErrorKind.NotFound, message);
    public static Error Forbidden(string? message = null) => new(ErrorKind.Forbidden, message);
    public static Error Conflict(string message) => new(ErrorKind.Conflict, message);
    public static Error Invalid(string message) => new(ErrorKind.Invalid, message);
}

/// <summary>What a handler returns: a value, or an error the endpoint turns into an HTTP status.</summary>
public readonly record struct Result<T>
{
    public T? Value { get; }
    public Error? Error { get; }

    private Result(T? value, Error? error)
    {
        Value = value;
        Error = error;
    }

    public static implicit operator Result<T>(T value) => new(value, null);
    public static implicit operator Result<T>(Error error) => new(default, error);
}

/// <summary>A handler outcome with no value.</summary>
public readonly record struct Result
{
    public Error? Error { get; }

    private Result(Error? error) => Error = error;

    public static Result Ok() => new(null);
    public static implicit operator Result(Error error) => new(error);
}

public static class ResultHttpExtensions
{
    public static IResult ToHttp<T>(this Result<T> result, Func<T, IResult>? onSuccess = null) =>
        result.Error is { } error ? error.ToHttp() : onSuccess?.Invoke(result.Value!) ?? TypedResults.Ok(result.Value);

    public static IResult ToHttp(this Result result, IResult? onSuccess = null) =>
        result.Error is { } error ? error.ToHttp() : onSuccess ?? TypedResults.NoContent();

    public static IResult ToHttp(this Error error) => error.Kind switch
    {
        ErrorKind.NotFound => TypedResults.Problem(error.Message, statusCode: StatusCodes.Status404NotFound),
        ErrorKind.Forbidden => TypedResults.Problem(error.Message, statusCode: StatusCodes.Status403Forbidden),
        ErrorKind.Conflict => TypedResults.Problem(error.Message, statusCode: StatusCodes.Status409Conflict),
        _ => TypedResults.Problem(error.Message, statusCode: StatusCodes.Status400BadRequest),
    };
}
