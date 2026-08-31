using Microsoft.AspNetCore.Mvc;

namespace Shop.Api.Extensions;

public static class ApiBehaviorExtensions
{
    private const string JsonPathPrefix = "$.";

    public static IServiceCollection AddModelBindingErrorFormat(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation failed"
                };

                var fields = context.ModelState
                    .Where(entry => entry.Value?.Errors.Count > 0)
                    .Select(entry => NormalizeField(entry.Key))
                    .Where(field => field.Length > 0)
                    .Distinct()
                    .ToArray();

                if (fields.Length == 0)
                    fields = [string.Empty];

                problemDetails.Extensions["errors"] = fields
                    .Select(field => new
                    {
                        field,
                        code = "request.invalid",
                        message = "Invalid value"
                    })
                    .ToArray();

                return new ObjectResult(problemDetails) { StatusCode = problemDetails.Status };
            };
        });

        return services;
    }

    private static string NormalizeField(string key)
        => key.StartsWith(JsonPathPrefix, StringComparison.Ordinal)
            ? ToCamelCase(key[JsonPathPrefix.Length..])
            : string.Empty;

    private static string ToCamelCase(string propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return propertyName;

        return string.Join('.', propertyName
            .Split('.')
            .Select(segment => segment.Length == 0
                ? segment
                : char.ToLowerInvariant(segment[0]) + segment[1..]));
    }
}
