using CSharpFunctionalExtensions;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;

namespace Shop.Domain.RefreshTokens;

public sealed class RefreshToken : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public Guid? ReplacedByTokenId { get; private set; }
    public TokenHash TokenHash { get; private set; } = default!;
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset ExpiresAt { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public bool IsRevoked => RevokedAt is not null;

    private RefreshToken() { }

    private RefreshToken(
        Guid id,
        Guid userId, 
        TokenHash tokenHash,
        DateTimeOffset createdAt,
        DateTimeOffset expiresAt)
    {
        Id = id;
        UserId = userId;
        TokenHash = tokenHash;
        CreatedAt = createdAt;
        ExpiresAt = expiresAt;
    }

    public static RefreshToken Create(
        Guid userId,
        TokenHash tokenHash,
        DateTimeOffset createdAt, 
        TimeSpan lifetime)
    {
        ArgumentNullException.ThrowIfNull(tokenHash);

        if (userId == Guid.Empty)
            throw new ArgumentException("User id must not be empty", nameof(userId));

        if (lifetime <= TimeSpan.Zero)
            throw new ArgumentException("Lifetime must be greater than zero", nameof(lifetime));

        return new RefreshToken(
            Guid.CreateVersion7(),
            userId,
            tokenHash,
            createdAt,
            createdAt + lifetime);
    }

    public bool IsExpired(DateTimeOffset now) => ExpiresAt <= now;
    public bool IsActive(DateTimeOffset now) => !IsRevoked && !IsExpired(now);

    public UnitResult<Error> Revoke(DateTimeOffset revokedAt)
    {
        if (IsRevoked)
            return DomainErrors.RefreshTokens.AlreadyRevoked();

        RevokedAt = revokedAt;

        return default;
    }

    public UnitResult<Error> ReplaceWith(Guid newTokenId, DateTimeOffset revokedAt)
    {
        if (newTokenId == Guid.Empty)
            throw new ArgumentException("New token id must not be empty", nameof(newTokenId));

        var revokeResult = Revoke(revokedAt);
        if (revokeResult.IsFailure)
            return revokeResult;

        ReplacedByTokenId = newTokenId;

        return default;
    }
}
