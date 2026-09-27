using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Domain.Common;
using Shop.Domain.Countries;

namespace Shop.Persistence.Repositories;

internal sealed class CountryRepository(AppDbContext context) : ICountryRepository
{
    public async Task<Maybe<Country>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Countries.FindAsync([id], cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Countries.AnyAsync(c => c.Id == id, cancellationToken);

    public Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeCountryId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Country> query = context.Countries.Where(c => c.Name == name);

        if (excludeCountryId.HasValue)
            query = query.Where(c => c.Id != excludeCountryId.Value);

        return query.AnyAsync(cancellationToken);
    }

    public Task<bool> ExistsBySlugAsync(
        Slug slug,
        Guid? excludeCountryId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Country> query = context.Countries.Where(c => c.Slug == slug);

        if (excludeCountryId.HasValue)
            query = query.Where(c => c.Id != excludeCountryId.Value);

        return query.AnyAsync(cancellationToken);
    }

    public void Add(Country country) => context.Countries.Add(country);
}