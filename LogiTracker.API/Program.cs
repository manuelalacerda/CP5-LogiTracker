using System.Reflection;
using LogiTracker.API.Exceptions;
using LogiTracker.API.Extensions;
using LogiTracker.API.Health;
using LogiTracker.Application.Services;
using LogiTracker.Application.Services.Implementations;
using LogiTracker.Infrastructure;
using LogiTracker.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Asp.Versioning;
using Microsoft.AspNetCore.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Swagger
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "LogiTracker API - v1",
        Version = "v1",
        Description = "API de gerenciamento logístico (v1 - legada)"
    });

    options.SwaggerDoc("v2", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "LogiTracker API - v2",
        Version = "v2",
        Description = "API de gerenciamento logístico (v2 - atual)"
    });

    options.DocInclusionPredicate((docName, apiDesc) =>
    {
        var metadata = apiDesc.ActionDescriptor.EndpointMetadata;

        if (metadata.Any(m => m is ApiVersionNeutralAttribute))
            return true;

        var mapToVersions = metadata
            .Where(m => m is MapToApiVersionAttribute)
            .SelectMany(m => ((MapToApiVersionAttribute)m).Versions)
            .Select(v => $"v{v.MajorVersion}")
            .ToList();

        if (mapToVersions.Count > 0)
            return mapToVersions.Contains(docName);

        var controllerVersions = metadata
            .Where(m => m is ApiVersionAttribute)
            .SelectMany(m => ((ApiVersionAttribute)m).Versions)
            .Select(v => $"v{v.MajorVersion}")
            .ToList();

        return controllerVersions.Contains(docName);
    });
});

// Banco
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseOracle(
        builder.Configuration.GetConnectionString("DefaultConnection")
    );
});

// Repository genérico
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

// Repository
builder.Services.AddScoped<ICargoRepository, CargoRepository>();

builder.Services.AddScoped<ICarrierRepository, CarrierRepository>();

builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();

builder.Services.AddScoped<IDriverRepository, DriverRepository>();

builder.Services.AddScoped<IVehicleRepository, VehicleRepository>();

// Serviço de aplicação (usa o repositório genérico para validar dependências)
builder.Services.AddScoped<IDeliveryService, DeliveryService>();

// Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddProblemDetails();

// Health checks (CP4): self + banco (Oracle, via DbContext do CP2)
builder.Services.AddLogiTrackerHealthChecks();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("escrita", opt =>
    {
        opt.PermitLimit = 10;                  // Máximo de 10 requisições
        opt.Window = TimeSpan.FromMinutes(1);  // Janela de 1 minuto
        opt.QueueLimit = 0;
    });
});

var app = builder.Build();

// Tratamento global
app.UseExceptionHandler();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "LogiTracker.API v1");
        options.SwaggerEndpoint("/swagger/v2/swagger.json", "LogiTracker.API v2");
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseRateLimiter(); 
app.MapControllers();

app.MapControllers();

// GET /health — único endpoint de health check, não listado no Swagger.
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteJsonResponse
});

app.Run();