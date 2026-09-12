using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Application.Options;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;
using Shop.Domain.RefreshTokens;
using Shop.Domain.Users;

namespace Shop.Application.Users.LoginUser;

public sealed class LoginUserCommandHandler
    (IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IJwtProvider jwtProvider,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    RefreshTokenOptions refreshTokenOptions) 
        : ICommandHandler<LoginUserCommand, LoginUserResponse>
{
    public async Task<Result<LoginUserResponse, Error>> HandleAsync
        (LoginUserCommand command, CancellationToken cancellationToken)
    {
        var phoneResult = Phone.Create(command.Phone);
        if (phoneResult.IsFailure)
        {
            passwordHasher.VerifyDummy(command.Password!);
            return DomainErrors.Auth.InvalidCredentials();
        }

        var phone = phoneResult.Value;

        var maybeUser = await userRepository.GetByPhoneAsync(phone, cancellationToken);

        if (maybeUser.HasNoValue)
        {
            passwordHasher.VerifyDummy(command.Password!);
            return DomainErrors.Auth.InvalidCredentials();
        }

        var user = maybeUser.Value;

        if (!passwordHasher.Verify(command.Password!, user.PasswordHash.Value))
            return DomainErrors.Auth.InvalidCredentials();

        var accessToken = jwtProvider.GenerateAccessToken(user);
        var generatedRefreshToken = refreshTokenGenerator.Generate();
        var refreshTokenHash = TokenHash.Create(generatedRefreshToken.Hash);
        var lifetime = TimeSpan.FromDays(refreshTokenOptions.LifetimeDays);

        var refreshToken = RefreshToken
            .Create(user.Id, refreshTokenHash.Value, timeProvider.GetUtcNow(), lifetime);

        refreshTokenRepository.Add(refreshToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new LoginUserResponse(accessToken, generatedRefreshToken.Value);
    }
}
