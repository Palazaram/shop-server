namespace Shop.Application.Users.ChangePassword;

public sealed record ChangePasswordRequest(string? CurrentPassword, string? NewPassword);
