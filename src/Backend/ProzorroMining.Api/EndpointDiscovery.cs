using ProzorroMining.App.Features.Analytics;
using ProzorroMining.App.Features.Imports;
using ProzorroMining.App.Features.System;

namespace ProzorroMining.Api;

/// <summary>
/// Registers all vertical slice endpoints.
/// </summary>
internal static class EndpointDiscovery
{
    /// <summary>
    /// Maps all vertical slice endpoints to the route builder.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    public static void MapVerticalSlices(this IEndpointRouteBuilder endpoints)
    {
        // System endpoints
        endpoints.MapGetSystemPing();

        // Import endpoints
        endpoints.MapGetImportStatus();
        endpoints.MapRunImport();

        // Analytics endpoints
        endpoints.MapGetDashboardOverview();
    }

    // System endpoints
    private static void MapGetSystemPing(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/api/v1/system/ping", async (GetSystemPing.Handler handler, CancellationToken cancellationToken) =>
            {
                var response = await handler.HandleAsync(new GetSystemPing.Request(), cancellationToken);
                return response.ToHttpResult();
            })
            .WithName("GetSystemPing")
            .WithDescription("Get system status")
            .WithOpenApi()
            .Produces<object>(StatusCodes.Status200OK);
    }

    // Import endpoints
    private static void MapGetImportStatus(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/api/v1/import/status", async (GetImportStatus.Handler handler, CancellationToken cancellationToken) =>
            {
                var response = await handler.HandleAsync(new GetImportStatus.Request(), cancellationToken);
                return response.ToHttpResult();
            })
            .WithName("GetImportStatus")
            .WithDescription("Get current import status")
            .WithOpenApi()
            .Produces<object>(StatusCodes.Status200OK);
    }

    private static void MapRunImport(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapPost("/api/v1/import/run", async (RunImport.Command command, RunImport.Handler handler, CancellationToken cancellationToken) =>
            {
                var response = await handler.HandleAsync(command, cancellationToken);
                return response.ToHttpResult(StatusCodes.Status202Accepted);
            })
            .WithName("RunImport")
            .WithDescription("Run an import of Prozorro data")
            .WithOpenApi()
            .Produces<object>(StatusCodes.Status202Accepted)
            .Accepts<RunImport.Command>("application/json");
    }

    // Analytics endpoints
    private static void MapGetDashboardOverview(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/api/v1/analytics/dashboard", async (GetDashboardOverview.Handler handler, CancellationToken cancellationToken) =>
            {
                var response = await handler.HandleAsync(new GetDashboardOverview.Request(), cancellationToken);
                return response.ToHttpResult();
            })
            .WithName("GetDashboardOverview")
            .WithDescription("Get dashboard overview with savings and top entities")
            .WithOpenApi()
            .Produces<object>(StatusCodes.Status200OK);
    }
}
