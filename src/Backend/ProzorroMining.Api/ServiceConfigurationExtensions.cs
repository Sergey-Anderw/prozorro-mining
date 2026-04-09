using System.Text.Json;
using System.Text.Json.Serialization;
using ProzorroMining.App.Features.Imports;

namespace ProzorroMining.Api;

public static class ServiceConfigurationExtensions
{
   
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

    public static IServiceCollection AddDatabaseHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks();
        return services;
    }

    public static IServiceCollection AddImportExecutionQueue(this IServiceCollection services)
    {
        services.AddSingleton<ImportExecutionQueue>();
        services.AddSingleton<IImportExecutionQueue>(sp => sp.GetRequiredService<ImportExecutionQueue>());
        services.AddHostedService(sp => sp.GetRequiredService<ImportExecutionQueue>());
        return services;
    }
}
