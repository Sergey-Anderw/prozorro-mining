using ProzorroMining.Contracts;

namespace ProzorroMining.Api;

/// <summary>
/// Extension methods for mapping domain Result types to HTTP IResult responses.
/// </summary>
internal static class ResultExtensions
{
    /// <summary>
    /// Maps a domain Result to an HTTP IResult response.
    /// Success: 200 OK
    /// Validation failure: 400 BadRequest
    /// Not found: 404 NotFound
    /// Conflict: 409 Conflict
    /// Unexpected: 500 InternalServerError
    /// </summary>
    public static IResult ToHttpResult(this Result result)
    {
        if (result.IsSuccess)
            return Results.Ok();

        if (result.Error == null)
            return Results.StatusCode(500);

        return result.Error.Code switch
        {
            "VALIDATION_FAILED" => Results.BadRequest(new
            {
                status = 400,
                title = "Validation Failed",
                detail = result.Error.Message,
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            }),
            "NOT_FOUND" => Results.NotFound(new
            {
                status = 404,
                title = "Not Found",
                detail = result.Error.Message,
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            }),
            "CONFLICT" => Results.Conflict(new
            {
                status = 409,
                title = "Conflict",
                detail = result.Error.Message,
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
            }),
            _ => Results.StatusCode(500)
        };
    }

    /// <summary>
    /// Maps a generic domain Result to an HTTP IResult response with data.
    /// Success: 200 OK with data
    /// Validation failure: 400 BadRequest
    /// Not found: 404 NotFound
    /// Conflict: 409 Conflict
    /// Unexpected: 500 InternalServerError
    /// </summary>
    public static IResult ToHttpResult<T>(this Result<T> result, int? successStatusCode = null)
    {
        if (result.IsSuccess)
        {
            var statusCode = successStatusCode ?? StatusCodes.Status200OK;
            return statusCode == StatusCodes.Status202Accepted
                ? Results.Accepted(value: result.Data)
                : Results.Ok(result.Data);
        }

        if (result.Error == null)
            return Results.StatusCode(500);

        return result.Error.Code switch
        {
            "VALIDATION_FAILED" => Results.BadRequest(new
            {
                status = 400,
                title = "Validation Failed",
                detail = result.Error.Message,
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            }),
            "NOT_FOUND" => Results.NotFound(new
            {
                status = 404,
                title = "Not Found",
                detail = result.Error.Message,
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            }),
            "CONFLICT" => Results.Conflict(new
            {
                status = 409,
                title = "Conflict",
                detail = result.Error.Message,
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.8"
            }),
            _ => Results.StatusCode(500)
        };
    }
}
