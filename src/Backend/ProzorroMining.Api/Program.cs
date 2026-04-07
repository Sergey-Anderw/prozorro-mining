var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Map endpoints
var group = app.MapGroup("/api").WithName("API v1");

group.MapGet("/health", () => new { status = "healthy" })
    .WithName("HealthCheck")
    .WithOpenApi();

app.Run();
