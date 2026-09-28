using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;
using Shop.Domain.RefreshTokens;
using Shop.Domain.Users;

namespace Shop.Application.Users.ChangePassword;

internal sealed class ChangePasswordCommandHandler(
    IUserRepository userRepository,
    IRefreshTokenRepository refreshTokenRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
        : ICommandHandler<ChangePasswordCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        ChangePasswordCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<User> maybeUser = await userRepository.GetByIdAsync(command.UserId, cancellationToken);

        if (maybeUser.HasNoValue)
            return DomainErrors.Users.NotFound();

        User user = maybeUser.Value;

        if (!passwordHasher.Verify(command.CurrentPassword!, user.PasswordHash.Value))
            return DomainErrors.Users.CurrentPasswordIsWrong();

        Result<PasswordHash, Error> newHash =
            PasswordHash.Create(passwordHasher.Hash(command.NewPassword!));

        if (newHash.IsFailure)
            return newHash.Error;

        user.ChangePassword(newHash.Value);

        // Пароль меняют в том числе потому, что старый утёк. Поэтому все refresh-токены
        // гасятся: иначе чужая сессия переживёт смену пароля и будет обновляться дальше.
        // Своя сессия тоже закончится — это цена, о которой фронт предупреждает заранее.
        IReadOnlyList<RefreshToken> active =
            await refreshTokenRepository.GetNotRevokedByUserIdAsync(user.Id, cancellationToken);

        DateTimeOffset now = timeProvider.GetUtcNow();

        foreach (RefreshToken token in active)
            token.Revoke(now);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}
