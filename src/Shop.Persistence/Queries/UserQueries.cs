using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Users;

namespace Shop.Persistence.Queries;

internal sealed class UserQueries(AppDbContext context) : IUserQueries
{
    public async Task<Maybe<UserProfileResponse>> GetProfileAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Email, Phone и части FullName — конвертированные value object'ы, и `.Value` внутри
        // Select EF не переведёт. Поэтому из базы приходят сами объекты, а строки берутся
        // из них уже в памяти.
        var row = await (
            from user in context.Users.AsNoTracking()
            join role in context.Roles on user.RoleId equals role.Id
            where user.Id == userId
            select new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.Phone,
                RoleName = role.Name,
                user.CreatedAt
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
            return Maybe<UserProfileResponse>.None;

        return new UserProfileResponse(
            row.Id,
            row.FullName.FirstName.Value,
            row.FullName.LastName.Value,
            row.FullName.Patronymic.Value,
            row.Email.Value,
            row.Phone.Value,
            row.RoleName,
            row.CreatedAt);
    }
}
