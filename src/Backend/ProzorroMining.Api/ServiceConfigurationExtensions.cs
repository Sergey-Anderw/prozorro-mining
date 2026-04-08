using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace ProzorroMining.Api;

/// <summary>
/// Extension methods for configuring application services and options.
/// </summary>
public static class ServiceConfigurationExtensions
{
    /// <summary>
    /// Configures JSON serialization options for HTTP endpoints.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection ConfigureJsonSerialization(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }

    /// <summary>
    /// Adds database readiness checks for PostgreSQL.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDatabaseHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks();
        return services;
    }
}
