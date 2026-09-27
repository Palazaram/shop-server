using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using Shop.Api.Authentication;
using Shop.Api.BackgroundJobs;
using Shop.Api.ExceptionHandling;
using Shop.Api.Extensions;
using Shop.Api.Filters;
using Shop.Api.Middleware;
using Shop.Application;
using Shop.Infrastructure;
using Shop.Persistence;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateOnBuild = true;
    options.ValidateScopes = true;
});

builder.Logging.ClearProviders();

builder.Services.AddSerilog((services, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(builder.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

builder.Services
    .AddApplication(builder.Configuration)
    .AddInfrastructure(builder.Configuration)
    .AddPersistence(connectionString);

builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>())
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
    .AddControllersAsServices();

builder.Services.AddModelBindingErrorFormat();

builder.Services.AddExceptionHandler<DuplicateKeyExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddFrontendCors(builder.Configuration);
builder.Services.AddOpenApiDocumentation();
builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddAuthorization();

builder.Services.AddSingleton<AuthCookieService>();

builder.Services.AddHostedService<RefreshTokenCleanupService>();

var app = builder.Build();

app.EnsureBodyParametersHaveValidators();

app.UseMiddleware<RequestTraceMiddleware>();

app.UseExceptionHandler();

app.UseSerilogRequestLogging(options => options.GetLevel = (context, _, exception) =>
    exception is not null || context.Response.StatusCode >= 500
        ? LogEventLevel.Error
        : context.Request.Path.StartsWithSegments("/health")
            ? LogEventLevel.Verbose
            : LogEventLevel.Information);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseFrontendCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false })
    .ExcludeFromDescription();

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
}).ExcludeFromDescription();

app.Run();
