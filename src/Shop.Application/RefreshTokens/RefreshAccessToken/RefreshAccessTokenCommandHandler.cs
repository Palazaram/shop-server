using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Application.Options;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;
using Shop.Domain.RefreshTokens;
using Shop.Domain.Users;

namespace Shop.Application.RefreshTokens.RefreshAccessToken;

public sealed class RefreshAccessTokenCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IUserRepository userRepository,
    IRefreshTokenGenerator refreshTokenGenerator,
    IJwtProvider jwtProvider,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    RefreshTokenOptions refreshTokenOptions)
        : ICommandHandler<RefreshAccessTokenCommand, RefreshAccessTokenResponse>
{
    public async Task<Result<RefreshAccessTokenResponse, Error>> HandleAsync(
        RefreshAccessTokenCommand command, CancellationToken cancellationToken)
    {
        string hash = refreshTokenGenerator.ComputeHash(command.RefreshToken);

        var tokenHashResult = TokenHash.Create(hash);
        if (tokenHashResult.IsFailure)
            return DomainErrors.Auth.RefreshTokenInvalid();

        var maybeStoredToken = await refreshTokenRepository
            .GetByTokenHashAsync(tokenHashResult.Value, cancellationToken);

        if (maybeStoredToken.HasNoValue)
            return DomainErrors.Auth.RefreshTokenInvalid();

        var storedToken = maybeStoredToken.Value;
        var now = timeProvider.GetUtcNow();

        // Предъявлен уже отозванный токен — признак утечки: гасим все сессии.
        if (storedToken.IsRevoked)
        {
            await RevokeAllUserTokensAsync(storedToken.UserId, now, cancellationToken);
            return DomainErrors.Auth.RefreshTokenInvalid();
        }

        if (storedToken.IsExpired(now))
            return DomainErrors.Auth.RefreshTokenInvalid();

        var maybeUser = await userRepository.GetByIdAsync(storedToken.UserId, cancellationToken);
        if (maybeUser.HasNoValue)
            return DomainErrors.Auth.RefreshTokenInvalid();

        var user = maybeUser.Value;

        var generatedRefreshToken = refreshTokenGenerator.Generate();
        var newTokenHash = TokenHash.Create(generatedRefreshToken.Hash);

        var newRefreshToken = RefreshToken.Create(
            user.Id,
            newTokenHash.Value,
            now,
            TimeSpan.FromDays(refreshTokenOptions.LifetimeDays));

        refreshTokenRepository.Add(newRefreshToken);

        var replaceResult = storedToken.ReplaceWith(newRefreshToken.Id, now);
        if (replaceResult.IsFailure)
            return replaceResult.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        string accessToken = jwtProvider.GenerateAccessToken(user);

        return new RefreshAccessTokenResponse(accessToken, generatedRefreshToken.Value);
    }

    private async Task RevokeAllUserTokensAsync(Guid userId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var tokens = await refreshTokenRepository
            .GetNotRevokedByUserIdAsync(userId, cancellationToken);

        foreach (var token in tokens)
            token.Revoke(now);

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}