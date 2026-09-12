namespace Shop.Api.Contracts;

public sealed record CurrentUserResponse(Guid UserId, string Role);