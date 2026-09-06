using Microsoft.IdentityModel.Tokens;
using Shop.Application.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace Shop.Infrastructure.Security;

public sealed class RefreshTokenGenerator : IRefreshTokenGenerator
{
    private const int TokenSizeInBytes = 32;

    public GeneratedRefreshToken Generate()
    {
        byte[] randomBytes = RandomNumberGenerator.GetBytes(TokenSizeInBytes);
        string value = Base64UrlEncoder.Encode(randomBytes);

        return new GeneratedRefreshToken(value, ComputeHash(value));
    }

    public string ComputeHash(string rawToken)
    {
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));

        return Convert.ToHexString(hashBytes);
    }
}