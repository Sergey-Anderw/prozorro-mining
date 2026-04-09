using ProzorroMining.Contracts;

namespace ProzorroMining.Api;

internal static class ResultExtensions
{
   
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
