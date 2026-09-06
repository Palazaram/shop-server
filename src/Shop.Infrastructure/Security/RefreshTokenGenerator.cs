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

        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        string hash = Convert.ToHexString(hashBytes);

        return new GeneratedRefreshToken(value, hash);
    }
}