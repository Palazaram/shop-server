using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Domain.Common;
using Shop.Domain.ProductAttributes;

namespace Shop.Persistence.Repositories;

internal sealed class ProductAttributeRepository(AppDbContext context) : IProductAttributeRepository
{
    public async Task<Maybe<ProductAttribute>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => await context.ProductAttributes.FindAsync([id], cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => context.ProductAttributes.AnyAsync(a => a.Id == id, cancellationToken);

    public Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeAttributeId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<ProductAttribute> query = context.ProductAttributes
            .Where(a => a.Name == name);

        if (excludeAttributeId.HasValue)
            query = query.Where(a => a.Id != excludeAttributeId.Value);

        return query.AnyAsync(cancellationToken);
    }

    public Task<bool> ExistsBySlugAsync(Slug slug, CancellationToken cancellationToken = default)
        => context.ProductAttributes.AnyAsync(a => a.Slug == slug, cancellationToken);

    public void Add(ProductAttribute attribute) => context.ProductAttributes.Add(attribute);
}