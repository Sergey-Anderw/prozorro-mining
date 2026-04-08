namespace ProzorroMining.Contracts;

/// <summary>
/// Represents an application error with code and message.
/// </summary>
public sealed record ApplicationError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApplicationError"/> record.
    /// </summary>
    /// <param name="code">The error code (e.g., "VALIDATION_FAILED", "NOT_FOUND").</param>
    /// <param name="message">The error message.</param>
    public ApplicationError(string code, string message)
    {
        Code = code;
        Message = message;
    }

    /// <summary>
    /// The error code (e.g., "VALIDATION_FAILED", "NOT_FOUND").
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// The error message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Creates a validation error.
    /// </summary>
    public static ApplicationError ValidationFailed(string message) =>
        new("VALIDATION_FAILED", message);

    /// <summary>
    /// Creates a not found error.
    /// </summary>
    public static ApplicationError NotFound(string message) =>
        new("NOT_FOUND", message);

    /// <summary>
    /// Creates a conflict error.
    /// </summary>
    public static ApplicationError Conflict(string message) =>
        new("CONFLICT", message);

    /// <summary>
    /// Creates an unexpected error.
    /// </summary>
    public static ApplicationError Unexpected(string message) =>
        new("UNEXPECTED_ERROR", message);
}
