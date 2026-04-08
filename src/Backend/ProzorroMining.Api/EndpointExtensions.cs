using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;
using System.Linq;

namespace ProzorroMining.Api;

/// <summary>
/// Extension methods for configuring API endpoints.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Maps all API v1 endpoints.
    /// </summary>
    /// <param name="app">The WebApplication.</param>
    public static void MapApiEndpoints(this WebApplication app)
    {
        // Map error handler
        app.MapGet("/error", HandleError)
            .ExcludeFromDescription();

        // Map health checks with OpenAPI support
        app.MapGet("/health/live", async (HealthCheckService healthCheckService) =>
        {
            var result = await healthCheckService.CheckHealthAsync(predicate: _ => false);
            return Results.Text(result.Status.ToString());
        })
            .WithName("HealthLive")
            .WithDescription("Liveness probe - indicates if the application is running")
            .Produces(200, contentType: "text/plain")
            .WithOpenApi();

        app.MapGet("/health/ready", async (HealthCheckService healthCheckService) =>
        {
            var result = await healthCheckService.CheckHealthAsync(predicate: _ => true);
            return Results.Text(result.Status.ToString());
        })
            .WithName("HealthReady")
            .WithDescription("Readiness probe - indicates if the application is ready to handle requests")
            .Produces(200, contentType: "text/plain")
            .WithOpenApi();

        // Discover and map all vertical slice endpoints
        app.MapVerticalSlices();
    }

    private static IResult HandleError(HttpContext context)
    {
        var exceptionHandler = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionHandler?.Error;

        if (exception is ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(x => x.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group.Select(x => x.ErrorMessage).ToArray());

            var problemDetails = new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                title = "One or more validation errors occurred.",
                status = StatusCodes.Status400BadRequest,
                errors,
                instance = context.Request.Path,
                traceId = Activity.Current?.Id ?? context.TraceIdentifier
            };

            return Results.Json(problemDetails, statusCode: StatusCodes.Status400BadRequest);
        }

        var fallbackProblemDetails = new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "An unexpected error occurred",
            status = StatusCodes.Status500InternalServerError,
            detail = context.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment()
                ? exception?.Message
                : "Internal server error",
            instance = context.Request.Path,
            traceId = Activity.Current?.Id ?? context.TraceIdentifier
        };

        return Results.Json(fallbackProblemDetails, statusCode: StatusCodes.Status500InternalServerError);
    }
}

