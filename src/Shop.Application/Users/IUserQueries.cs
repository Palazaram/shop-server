using CSharpFunctionalExtensions;

namespace Shop.Application.Users;

/// <summary>
/// Профиль текущего пользователя. Заменяет прежний `GET /api/auth/me`, который отдавал только
/// содержимое токена: двух разных «кто я» быть не должно — фронт не обязан выбирать, какой звать.
/// </summary>
public sealed record UserProfileResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Patronymic,
    string Email,
    string Phone,
    string Role,
    DateTimeOffset CreatedAt);

public interface IUserQueries
{
    Task<Maybe<UserProfileResponse>> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken);
}
