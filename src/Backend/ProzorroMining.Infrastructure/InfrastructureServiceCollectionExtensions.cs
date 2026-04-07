using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ProzorroMining.Infrastructure;

/// <summary>
/// Extension methods for configuring the infrastructure layer services.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Adds infrastructure layer services to the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Infrastructure services (database, external APIs, repositories, etc.)
        // Will be added here as needed
        // Example: services.AddScoped<IUserRepository, UserRepository>();
        
        return services;
    }
}
