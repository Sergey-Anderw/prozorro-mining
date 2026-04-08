namespace ProzorroMining.Api;

/// <summary>
/// Extension methods for configuring the API layer services.
/// </summary>
public static class ApiServiceCollectionExtensions
{
    /// <summary>
    /// Adds API layer services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        // API layer services (endpoints, request handlers, etc.)
        // Will be added here as the API grows
        
        return services;
    }
}
