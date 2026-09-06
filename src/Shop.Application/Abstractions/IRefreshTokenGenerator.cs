namespace Shop.Application.Abstractions;

public sealed record GeneratedRefreshToken(string Value, string Hash);
public interface IRefreshTokenGenerator
{
    GeneratedRefreshToken Generate();
    string ComputeHash(string rawToken);
}