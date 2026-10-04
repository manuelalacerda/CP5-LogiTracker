using System.Threading.RateLimiting;
using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using LogiTracker.API.Exceptions;
using LogiTracker.API.Extensions;
using LogiTracker.API.Health;
using LogiTracker.API.Swagger;
using LogiTracker.Application.Services;
using LogiTracker.Application.Services.Implementations;
using LogiTracker.Infrastructure;
using LogiTracker.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Versionamento (CP5 - A)
builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(2, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = ApiVersionReader.Combine(
            new QueryStringApiVersionReader("api-version"),
            new HeaderApiVersionReader("X-Api-Version"));
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVVV";   // v1.0 e v2.0
    });

// Swagger: um documento por versão
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

// Banco
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseOracle(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        oracleOptions =>
        {
            oracleOptions.UseOracleSQLCompatibility(
                OracleSQLCompatibility.DatabaseVersion19);
        }
    );
});

// Repository genérico
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Repositories
builder.Services.AddScoped<ICargoRepository, CargoRepository>();
builder.Services.AddScoped<ICarrierRepository, CarrierRepository>();
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IDriverRepository, DriverRepository>();
builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();

// Serviço de aplicação
builder.Services.AddScoped<IDeliveryService, DeliveryService>();

// Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// Health checks (CP4)
builder.Services.AddLogiTrackerHealthChecks();

// Rate limit (CP5 - C)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("escrita", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "anon",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.OnRejected = async (context, ct) =>
    {
        var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var ts)
            ? (int)Math.Ceiling(ts.TotalSeconds)
            : 60;

        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

        await context.HttpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = 429,
            Title = "Too Many Requests",
            Detail = $"Limite de 10 requisições por minuto excedido. Tente novamente em {retryAfter}s.",
            Instance = context.HttpContext.Request.Path
        }, ct);
    };
});

var app = builder.Build();

// Tratamento global (uma única vez)
app.UseExceptionHandler();

// Swagger (Development)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
        foreach (var d in provider.ApiVersionDescriptions.Reverse())
        {
            var nome = $"LogiTracker {d.GroupName}" + (d.IsDeprecated ? " (deprecada)" : "");
            options.SwaggerEndpoint($"/swagger/{d.GroupName}/swagger.json", nome);
        }
    });
}

//app.UseHttpsRedirection();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

// /health fora do teto de taxa
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
}).DisableRateLimiting();

app.Run();