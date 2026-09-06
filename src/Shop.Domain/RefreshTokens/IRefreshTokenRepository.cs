using CSharpFunctionalExtensions;

namespace Shop.Domain.RefreshTokens;

public interface IRefreshTokenRepository
{
    Task<Maybe<RefreshToken>> GetByTokenHashAsync(TokenHash tokenHash, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RefreshToken>> GetNotRevokedByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    void Add(RefreshToken refreshToken);
}
