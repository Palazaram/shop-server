using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Domain.Users;

namespace Shop.Persistence.Repositories;

internal sealed class UserRepository(AppDbContext context) : IUserRepository
{
    public void Add(User user) => context.Users.Add(user);

    public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
        => context.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public Task<bool> ExistsByPhoneAsync(Phone phone, CancellationToken cancellationToken = default)
        => context.Users.AnyAsync(u => u.Phone == phone, cancellationToken);

    public async Task<Maybe<User>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Users.FindAsync([id], cancellationToken);
}