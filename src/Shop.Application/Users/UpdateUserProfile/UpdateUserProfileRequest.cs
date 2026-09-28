namespace Shop.Application.Users.UpdateUserProfile;

public sealed record UpdateUserProfileRequest(
    string? FirstName,
    string? LastName,
    string? Patronymic,
    string? Phone);
