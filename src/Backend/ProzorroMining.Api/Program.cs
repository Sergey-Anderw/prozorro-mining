using Serilog;
using ProzorroMining.Api;
using ProzorroMining.App;
using ProzorroMining.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, configuration) =>
{
    configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("Application", "ProzorroMining.Api")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}");
});

// Configure services
builder.Services
    .AddSwaggerConfiguration()
    .ConfigureJsonSerialization()
    .AddDatabaseHealthChecks(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddApplication()
    .AddApi();

var app = builder.Build();

// Configure middleware
app.UseSwaggerConfiguration();
app.UseExceptionHandler("/error");
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) =>
        ex != null ? Serilog.Events.LogEventLevel.Error :
        httpContext.Response.StatusCode >= 500 ? Serilog.Events.LogEventLevel.Error :
        httpContext.Response.StatusCode >= 400 ? Serilog.Events.LogEventLevel.Warning :
        Serilog.Events.LogEventLevel.Information;
});
app.UseHttpsRedirection();

// Map endpoints
app.MapApiEndpoints();

app.Run();
