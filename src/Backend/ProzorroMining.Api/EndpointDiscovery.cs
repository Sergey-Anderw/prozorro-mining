using ProzorroMining.App.Features.Analytics;
using ProzorroMining.App.Features.Imports;

namespace ProzorroMining.Api;
internal static class EndpointDiscovery
{
    public static void MapVerticalSlices(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGetImportStatus();
        endpoints.MapRunImport();
        endpoints.MapGetBudgetSavings();
        endpoints.MapGetTopProcurers();
        endpoints.MapGetTopSuppliers();
    }

    
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
            .Produces<GetImportStatus.Response>(StatusCodes.Status200OK);
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
            .WithDescription("Start an import of Prozorro data")
            .WithOpenApi()
            .Produces<RunImport.Response>(StatusCodes.Status202Accepted)
            .Accepts<RunImport.Command>("application/json");
    }

    
    private static void MapGetBudgetSavings(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/api/v1/analytics/savings", async (GetBudgetSavings.Handler handler, CancellationToken cancellationToken) =>
            {
                var response = await handler.HandleAsync(new GetBudgetSavings.Request(), cancellationToken);
                return response.ToHttpResult();
            })
            .WithName("GetBudgetSavings")
            .WithDescription("Get total budget savings")
            .WithOpenApi()
            .Produces<GetBudgetSavings.Response>(StatusCodes.Status200OK);
    }

    private static void MapGetTopProcurers(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/api/v1/analytics/top-procurers", async (GetTopProcurers.Handler handler, CancellationToken cancellationToken) =>
            {
                var response = await handler.HandleAsync(new GetTopProcurers.Request(), cancellationToken);
                return response.ToHttpResult();
            })
            .WithName("GetTopProcurers")
            .WithDescription("Get top 5 procurers by contract value")
            .WithOpenApi()
            .Produces<IReadOnlyList<GetTopProcurers.ProcurerInfo>>(StatusCodes.Status200OK);
    }

    private static void MapGetTopSuppliers(this IEndpointRouteBuilder endpoints)
    {
        endpoints
            .MapGet("/api/v1/analytics/top-suppliers", async (GetTopSuppliers.Handler handler, CancellationToken cancellationToken) =>
            {
                var response = await handler.HandleAsync(new GetTopSuppliers.Request(), cancellationToken);
                return response.ToHttpResult();
            })
            .WithName("GetTopSuppliers")
            .WithDescription("Get top 5 suppliers by contract value")
            .WithOpenApi()
            .Produces<IReadOnlyList<GetTopSuppliers.SupplierInfo>>(StatusCodes.Status200OK);
    }
}
