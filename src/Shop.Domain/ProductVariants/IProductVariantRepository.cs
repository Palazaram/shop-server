using CSharpFunctionalExtensions;
using Shop.Domain.Common;

namespace Shop.Domain.ProductVariants;

public interface IProductVariantRepository
{
    Task<Maybe<ProductVariant>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySkuAsync(
        string sku,
        Guid? excludeVariantId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(Slug slug, CancellationToken cancellationToken = default);

    Task<bool> ExistsByPackagingAsync(
        Guid productId,
        Packaging packaging,
        CancellationToken cancellationToken = default);

    void Add(ProductVariant variant);
}