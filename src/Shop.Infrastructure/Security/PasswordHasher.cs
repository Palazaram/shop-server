using Shop.Application.Abstractions;

namespace Shop.Infrastructure.Security;

internal sealed class PasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;
    private static readonly string DummyHash =
        BCrypt.Net.BCrypt.HashPassword("dummy-password-for-timing-equalization", WorkFactor);

    public string Hash(string password)
        => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash)
        => BCrypt.Net.BCrypt.Verify(password, hash);

    public bool VerifyDummy(string password)
    {
        _ = BCrypt.Net.BCrypt.Verify(password, DummyHash);
        return false;
    }
}