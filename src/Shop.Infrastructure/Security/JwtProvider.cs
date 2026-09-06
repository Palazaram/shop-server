using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Shop.Application.Abstractions;
using Shop.Domain.Roles;
using Shop.Domain.Users;
using Shop.Infrastructure.Options;
using System.Security.Claims;
using System.Text;

namespace Shop.Infrastructure.Security;

public sealed class JwtProvider
    (JwtOptions jwtOptions, TimeProvider timeProvider) : IJwtProvider
{
    private static readonly JsonWebTokenHandler TokenHandler = new();

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimNames.Role, RoleNames.FromId(user.RoleId))
        };

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtOptions.SecretKey));

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = timeProvider.GetUtcNow().UtcDateTime.AddMinutes(jwtOptions.AccessTokenExpirationMinutes),
            Issuer = jwtOptions.Issuer,
            Audience = jwtOptions.Audience,
            SigningCredentials = new SigningCredentials(
                signingKey, SecurityAlgorithms.HmacSha256)
        };

        return TokenHandler.CreateToken(descriptor);
    }
}
