namespace ProzorroMining.Api;

/// <summary>
/// Extension methods for configuring Swagger/OpenAPI.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Adds and configures Swagger/OpenAPI services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "ProzorroMining API",
                Version = "v1",
                Description = "API for ProzorroMining - Modular Monolith Architecture"
            });
        });

        return services;
    }

    /// <summary>
    /// Configures Swagger/OpenAPI middleware.
    /// </summary>
    /// <param name="app">The WebApplication.</param>
    public static void UseSwaggerConfiguration(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "ProzorroMining API v1");
                options.RoutePrefix = "swagger";
            });
        }
    }
}
