using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;
using Shop.Domain.RefreshTokens;

namespace Shop.Application.RefreshTokens.RevokeRefreshToken;

public sealed class RevokeRefreshTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
        : ICommandHandler<RevokeRefreshTokenCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        RevokeRefreshTokenCommand command, CancellationToken cancellationToken)
    {
        string hash = refreshTokenGenerator.ComputeHash(command.RefreshToken);

        var tokenHashResult = TokenHash.Create(hash);
        if (tokenHashResult.IsFailure)
            return default;

        var maybeStoredToken = await refreshTokenRepository
            .GetByTokenHashAsync(tokenHashResult.Value, cancellationToken);

        if (maybeStoredToken.HasNoValue)
            return default;

        var storedToken = maybeStoredToken.Value;

        if (storedToken.IsRevoked)
            return default;

        storedToken.Revoke(timeProvider.GetUtcNow());

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return default;
    }
}