using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Domain.Common;
using Shop.Domain.Manufacturers;

namespace Shop.Persistence.Repositories;

internal sealed class ManufacturerRepository(AppDbContext context) : IManufacturerRepository
{
    public async Task<Maybe<Manufacturer>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => await context.Manufacturers.FindAsync([id], cancellationToken);

    public Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeManufacturerId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Manufacturer> query = context.Manufacturers.Where(m => m.Name == name);

        if (excludeManufacturerId.HasValue)
            query = query.Where(m => m.Id != excludeManufacturerId.Value);

        return query.AnyAsync(cancellationToken);
    }

    public Task<bool> ExistsBySlugAsync(Slug slug, CancellationToken cancellationToken = default)
        => context.Manufacturers.AnyAsync(m => m.Slug == slug, cancellationToken);

    public void Add(Manufacturer manufacturer) => context.Manufacturers.Add(manufacturer);
}