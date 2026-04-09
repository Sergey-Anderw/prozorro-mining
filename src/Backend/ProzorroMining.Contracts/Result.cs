namespace ProzorroMining.Contracts;

public sealed record Result
{
   
    public bool IsSuccess { get; init; }

    public ApplicationError? Error { get; init; }

    public static Result Ok() =>
        new() { IsSuccess = true, Error = null };

    public static Result Fail(ApplicationError error) =>
        new() { IsSuccess = false, Error = error };

    public static Result ValidationFailed(string message) =>
        Fail(ApplicationError.ValidationFailed(message));

    public static Result NotFound(string message) =>
        Fail(ApplicationError.NotFound(message));

    public static Result Conflict(string message) =>
        Fail(ApplicationError.Conflict(message));

    public static Result Unexpected(string message) =>
        Fail(ApplicationError.Unexpected(message));
}

public sealed record Result<T>
{
    
    public T? Data { get; init; }

    public bool IsSuccess { get; init; }

    public ApplicationError? Error { get; init; }

    public static Result<T> Ok(T data) =>
        new() { IsSuccess = true, Data = data, Error = null };

    public static Result<T> Fail(ApplicationError error) =>
        new() { IsSuccess = false, Data = default, Error = error };

    public static Result<T> ValidationFailed(string message) =>
        Fail(ApplicationError.ValidationFailed(message));

    public static Result<T> NotFound(string message) =>
        Fail(ApplicationError.NotFound(message));

    public static Result<T> Conflict(string message) =>
        Fail(ApplicationError.Conflict(message));

    public static Result<T> Unexpected(string message) =>
        Fail(ApplicationError.Unexpected(message));
}
