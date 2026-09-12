using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Shop.Api.Authentication;

namespace Shop.Api.Extensions;

public static class OpenApiExtensions
{
    private const string SecuritySchemeId = "CookieAuth";

    public static IServiceCollection AddOpenApiDocumentation(this IServiceCollection services)
    {
        services.AddOpenApi(options =>
        {
            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info = new OpenApiInfo
                {
                    Title = "Shop API",
                    Version = "v1"
                };

                document.Components ??= new OpenApiComponents();
                document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

                document.Components.SecuritySchemes[SecuritySchemeId] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.ApiKey,
                    In = ParameterLocation.Cookie,
                    Name = AuthCookieService.AccessTokenCookieName,
                    Description = "Access token is sent automatically in an httpOnly cookie."
                };

                return Task.CompletedTask;
            });

            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                var metadata = context.Description.ActionDescriptor.EndpointMetadata;

                bool allowsAnonymous = metadata.OfType<IAllowAnonymous>().Any();
                var authorizeData = metadata.OfType<IAuthorizeData>().ToArray();

                if (allowsAnonymous || authorizeData.Length == 0)
                    return Task.CompletedTask;

                operation.Security =
                [
                    new OpenApiSecurityRequirement
                    {
                        { new OpenApiSecuritySchemeReference(SecuritySchemeId, context.Document), [] }
                    }
                ];

                operation.Responses ??= new OpenApiResponses();
                operation.Responses["401"] = new OpenApiResponse { Description = "Unauthorized" };

                bool requiresRoleOrPolicy = authorizeData.Any(data =>
                    !string.IsNullOrEmpty(data.Roles) || !string.IsNullOrEmpty(data.Policy));

                if (requiresRoleOrPolicy)
                    operation.Responses["403"] = new OpenApiResponse { Description = "Forbidden" };

                return Task.CompletedTask;
            });
        });

        return services;
    }
}