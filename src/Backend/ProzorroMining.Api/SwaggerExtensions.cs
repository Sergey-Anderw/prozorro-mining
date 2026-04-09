namespace ProzorroMining.Api;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.CustomSchemaIds(type => (type.FullName ?? type.Name).Replace("+", "."));
            options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "ProzorroMining API",
                Version = "v1",
                Description = "API for ProzorroMining - Modular Monolith Architecture"
            });
        });

        return services;
    }

    public static void UseSwaggerConfiguration(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "ProzorroMining API v1");
            options.RoutePrefix = "swagger";
        });
    }
}
