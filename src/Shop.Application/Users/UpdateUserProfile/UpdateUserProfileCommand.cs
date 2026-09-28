namespace Shop.Application.Users.UpdateUserProfile;

public sealed record UpdateUserProfileCommand(
    Guid UserId,
    string? FirstName,
    string? LastName,
    string? Patronymic,
    string? Phone);
