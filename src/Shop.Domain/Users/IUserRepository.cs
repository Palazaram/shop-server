using CSharpFunctionalExtensions;

namespace Shop.Domain.Users;

public interface IUserRepository
{
    Task<Maybe<User>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<bool> ExistsByPhoneAsync(Phone phone, CancellationToken cancellationToken = default);
    void Add(User user);
}
