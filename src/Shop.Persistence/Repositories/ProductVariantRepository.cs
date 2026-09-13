using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Domain.Common;
using Shop.Domain.ProductVariants;

namespace Shop.Persistence.Repositories;

internal sealed class ProductVariantRepository(AppDbContext context) : IProductVariantRepository
{
    public async Task<Maybe<ProductVariant>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.ProductVariants.FindAsync([id], cancellationToken);

    public Task<bool> ExistsBySkuAsync(
        string sku,
        Guid? excludeVariantId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<ProductVariant> query = context.ProductVariants.Where(v => v.Sku == sku);

        if (excludeVariantId.HasValue)
            query = query.Where(v => v.Id != excludeVariantId.Value);

        return query.AnyAsync(cancellationToken);
    }

    public Task<bool> ExistsBySlugAsync(Slug slug, CancellationToken cancellationToken = default)
        => context.ProductVariants.AnyAsync(v => v.Slug == slug, cancellationToken);

    public Task<bool> ExistsByPackagingAsync(Guid productId, Packaging packaging, CancellationToken cancellationToken = default)
        => context.ProductVariants.AnyAsync(
            v => v.ProductId == productId
                 && v.Packaging.Value == packaging.Value
                 && v.Packaging.Unit == packaging.Unit,
            cancellationToken);

    public void Add(ProductVariant variant) => context.ProductVariants.Add(variant);
}