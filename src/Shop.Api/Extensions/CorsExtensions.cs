namespace Shop.Api.Extensions;

public static class CorsExtensions
{
    public const string PolicyName = "frontend";

    private const string OriginsPath = "Cors:AllowedOrigins";

    public static IServiceCollection AddFrontendCors(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        string[] origins = ReadOrigins(configuration);

        if (origins.Length == 0)
            return services;

        services.AddCors(options => options.AddPolicy(PolicyName, policy => policy
            .WithOrigins(origins)
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod()));

        return services;
    }

    /// <summary>
    /// Пустой список — законное состояние: в проде фронт и API стоят за одним nginx,
    /// запросы не межсайтовые и CORS не нужен. Поэтому не падаем, а говорим об этом в лог.
    /// </summary>
    public static WebApplication UseFrontendCors(this WebApplication app)
    {
        if (ReadOrigins(app.Configuration).Length == 0)
        {
            app.Logger.LogInformation("CORS is off: {Path} is empty", OriginsPath);
            return app;
        }

        app.UseCors(PolicyName);

        return app;
    }

    private static string[] ReadOrigins(IConfiguration configuration)
        => configuration.GetSection(OriginsPath).Get<string[]>() ?? [];
}