using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using ProzorroMining.App.Abstractions.Persistence;
using ProzorroMining.App.Abstractions.Prozorro;
using ProzorroMining.Infrastructure.Configuration;
using ProzorroMining.Infrastructure.Db;
using ProzorroMining.Infrastructure.Prozorro;
using ProzorroMining.Infrastructure.Repositories;
using Npgsql;

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
        services
            .AddOptions<PostgresOptions>()
            .Bind(configuration.GetSection(PostgresOptions.SectionName))
            .ValidateDataAnnotations();

        services
            .AddOptions<ProzorroApiOptions>()
            .Bind(configuration.GetSection(ProzorroApiOptions.SectionName))
            .ValidateDataAnnotations();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? configuration["ConnectionStrings:DefaultConnection"];

        services.TryAddSingleton(sp =>
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
            }

            var options = sp.GetRequiredService<IOptions<PostgresOptions>>().Value;
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var normalizedConnectionString = NormalizeConnectionString(connectionString, options);
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(normalizedConnectionString);
            dataSourceBuilder.UseLoggerFactory(loggerFactory);
            return dataSourceBuilder.Build();
        });

        services.AddSingleton(sp =>
        {
            var options = sp.GetRequiredService<IOptions<PostgresOptions>>().Value;
            return new PostgresCommandSettings(options.CommandTimeoutSeconds);
        });

        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();

        services.AddScoped<IImportRunRepository, ImportRunRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<ITenderRepository, TenderRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IContractRepository, ContractRepository>();
        services.AddScoped<IProzorroApiParser, ProzorroApiParser>();

        services.AddHttpClient<IProzorroApiTransport, ProzorroApiTransport>((serviceProvider, client) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<ProzorroApiOptions>>().Value;
            var logger = serviceProvider.GetRequiredService<ILogger<ProzorroApiTransport>>();

            if (Uri.TryCreate(options.ApiUrl, UriKind.Absolute, out var baseAddress))
            {
                client.BaseAddress = baseAddress;
            }
            else
            {
                logger.LogWarning(
                    "Prozorro API base URL is not configured. External calls will fail until {Section}:ApiUrl is set.",
                    ProzorroApiOptions.SectionName);
            }

            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            if (!string.IsNullOrWhiteSpace(options.ApiKey))
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {options.ApiKey}");
            }
        })
        .AddResilienceHandler("prozorro-api", static (builder, context) =>
        {
            var options = context.ServiceProvider.GetRequiredService<IOptions<ProzorroApiOptions>>().Value;
            var logger = context.ServiceProvider.GetRequiredService<ILoggerFactory>()
                .CreateLogger("ProzorroApiRetry");

            builder.AddRetry(new HttpRetryStrategyOptions
            {
                MaxRetryAttempts = options.Retry.MaxAttempts,
                Delay = TimeSpan.FromSeconds(options.Retry.BaseDelaySeconds),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                    .Handle<HttpRequestException>()
                    .HandleResult(response =>
                        response.StatusCode == System.Net.HttpStatusCode.TooManyRequests ||
                        (int)response.StatusCode >= 500),
                OnRetry = args =>
                {
                    logger.LogWarning(
                        "Retrying Prozorro HTTP request. Attempt {Attempt} after {DelayMs} ms. Outcome: {Outcome}",
                        args.AttemptNumber + 1,
                        args.RetryDelay.TotalMilliseconds,
                        args.Outcome.Result?.StatusCode.ToString() ?? args.Outcome.Exception?.GetType().Name);
                    return default;
                }
            });
        });

        services.AddScoped<IProzorroApiClient>(sp => new ProzorroApiClient(
            sp.GetRequiredService<IProzorroApiTransport>(),
            sp.GetRequiredService<IProzorroApiParser>(),
            sp.GetRequiredService<ILogger<ProzorroApiClient>>()));

        return services;
    }

    private static string NormalizeConnectionString(string connectionString, PostgresOptions options)
    {
        var builder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Pooling = true,
            Timeout = options.ConnectionTimeoutSeconds,
            CommandTimeout = options.CommandTimeoutSeconds,
            MinPoolSize = options.MinimumPoolSize,
            MaxPoolSize = options.MaximumPoolSize,
            KeepAlive = options.KeepAliveSeconds
        };

        return builder.ConnectionString;
    }
}
