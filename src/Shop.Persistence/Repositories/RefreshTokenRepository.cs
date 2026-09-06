using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Domain.RefreshTokens;

namespace Shop.Persistence.Repositories;

internal sealed class RefreshTokenRepository(AppDbContext context) : IRefreshTokenRepository
{
    public void Add(RefreshToken refreshToken) => context.RefreshTokens.Add(refreshToken);

    public async Task<Maybe<RefreshToken>> GetByTokenHashAsync(TokenHash tokenHash, CancellationToken cancellationToken = default)
        => await context.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);
    
    public async Task<IReadOnlyList<RefreshToken>> GetNotRevokedByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);
}
