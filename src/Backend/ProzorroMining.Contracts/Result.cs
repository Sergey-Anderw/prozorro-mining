namespace ProzorroMining.Contracts;

/// <summary>
/// Represents the result of an operation without a return value.
/// Supports success and failure cases.
/// </summary>
public sealed record Result
{
    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// The error associated with a failed result. Null if successful.
    /// </summary>
    public ApplicationError? Error { get; init; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Ok() =>
        new() { IsSuccess = true, Error = null };

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result Fail(ApplicationError error) =>
        new() { IsSuccess = false, Error = error };

    /// <summary>
    /// Creates a failed result with validation error.
    /// </summary>
    public static Result ValidationFailed(string message) =>
        Fail(ApplicationError.ValidationFailed(message));

    /// <summary>
    /// Creates a failed result with not found error.
    /// </summary>
    public static Result NotFound(string message) =>
        Fail(ApplicationError.NotFound(message));

    /// <summary>
    /// Creates a failed result with conflict error.
    /// </summary>
    public static Result Conflict(string message) =>
        Fail(ApplicationError.Conflict(message));

    /// <summary>
    /// Creates a failed result with unexpected error.
    /// </summary>
    public static Result Unexpected(string message) =>
        Fail(ApplicationError.Unexpected(message));
}

/// <summary>
/// Represents the result of an operation that returns a value.
/// Supports success and failure cases.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public sealed record Result<T>
{
    /// <summary>
    /// The result value. Only valid if IsSuccess is true.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// The error associated with a failed result. Null if successful.
    /// </summary>
    public ApplicationError? Error { get; init; }

    /// <summary>
    /// Creates a successful result with data.
    /// </summary>
    public static Result<T> Ok(T data) =>
        new() { IsSuccess = true, Data = data, Error = null };

    /// <summary>
    /// Creates a failed result with the specified error.
    /// </summary>
    public static Result<T> Fail(ApplicationError error) =>
        new() { IsSuccess = false, Data = default, Error = error };

    /// <summary>
    /// Creates a failed result with validation error.
    /// </summary>
    public static Result<T> ValidationFailed(string message) =>
        Fail(ApplicationError.ValidationFailed(message));

    /// <summary>
    /// Creates a failed result with not found error.
    /// </summary>
    public static Result<T> NotFound(string message) =>
        Fail(ApplicationError.NotFound(message));

    /// <summary>
    /// Creates a failed result with conflict error.
    /// </summary>
    public static Result<T> Conflict(string message) =>
        Fail(ApplicationError.Conflict(message));

    /// <summary>
    /// Creates a failed result with unexpected error.
    /// </summary>
    public static Result<T> Unexpected(string message) =>
        Fail(ApplicationError.Unexpected(message));
}
