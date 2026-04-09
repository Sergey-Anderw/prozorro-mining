namespace ProzorroMining.Contracts;

public sealed record ApplicationError(string Code, string Message)
{
    public static ApplicationError ValidationFailed(string message) =>
        new("VALIDATION_FAILED", message);

    public static ApplicationError NotFound(string message) =>
        new("NOT_FOUND", message);

    public static ApplicationError Conflict(string message) =>
        new("CONFLICT", message);

    public static ApplicationError Unexpected(string message) =>
        new("UNEXPECTED_ERROR", message);
}
