using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Domain.Users;

namespace Shop.Persistence.Repositories;

internal sealed class UserRepository(AppDbContext context) : IUserRepository
{
    public void Add(User user) => context.Users.Add(user);

    public Task<bool> ExistsByEmailAsync(Email email, CancellationToken cancellationToken = default)
        => context.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public Task<bool> ExistsByPhoneAsync(
        Phone phone,
        Guid? excludeUserId = null,
        CancellationToken cancellationToken = default)
    {
        IQueryable<User> query = context.Users.Where(u => u.Phone == phone);

        if (excludeUserId.HasValue)
            query = query.Where(u => u.Id != excludeUserId.Value);

        return query.AnyAsync(cancellationToken);
    }

    public async Task<Maybe<User>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Users.FindAsync([id], cancellationToken);

    public async Task<Maybe<User>> GetByPhoneAsync(Phone phone, CancellationToken cancellationToken = default)
        => await context.Users.FirstOrDefaultAsync(u => u.Phone == phone, cancellationToken);
}