using CSharpFunctionalExtensions;
using Shop.Domain.Common;

namespace Shop.Domain.ProductAttributes;

public interface IProductAttributeRepository
{
    Task<Maybe<ProductAttribute>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeAttributeId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(Slug slug, CancellationToken cancellationToken = default);

    Task<bool> AllExistAsync(IReadOnlyCollection<Guid> ids,CancellationToken cancellationToken = default);

    void Add(ProductAttribute attribute);
}