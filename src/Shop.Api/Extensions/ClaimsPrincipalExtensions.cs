using Microsoft.IdentityModel.JsonWebTokens;
using Shop.Application.Abstractions;
using System.Security.Claims;

namespace Shop.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        string? subject = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);

        if (!Guid.TryParse(subject, out Guid userId))
            throw new InvalidOperationException("Authenticated principal has no valid 'sub' claim.");

        return userId;
    }

    public static string GetRole(this ClaimsPrincipal principal)
        => principal.FindFirstValue(ClaimNames.Role)
           ?? throw new InvalidOperationException("Authenticated principal has no 'role' claim.");
}