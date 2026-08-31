using Scalar.AspNetCore;
using Shop.Api.ExceptionHandling;
using Shop.Api.Extensions;
using Shop.Api.Filters;
using Shop.Application;
using Shop.Infrastructure;
using Shop.Persistence;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Database")
    ?? throw new InvalidOperationException("Connection string 'Database' is not configured.");

builder.Services
    .AddApplication()
    .AddInfrastructure()
    .AddPersistence(connectionString);

builder.Services.AddControllers(options => options.Filters.Add<ValidationFilter>());
builder.Services.AddModelBindingErrorFormat();

builder.Services.AddExceptionHandler<DuplicateKeyExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
