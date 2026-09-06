using CSharpFunctionalExtensions;

namespace Shop.Domain.RefreshTokens;

public interface IRefreshTokenRepository
{
    Task<Maybe<RefreshToken>> GetByTokenHashAsync(TokenHash tokenHash, CancellationToken cancellationToken = default);
    void Add(RefreshToken refreshToken);
}
