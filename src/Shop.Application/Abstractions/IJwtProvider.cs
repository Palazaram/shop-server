using Shop.Domain.Users;

namespace Shop.Application.Abstractions;

public interface IJwtProvider
{
    string GenerateAccessToken(User user);
}