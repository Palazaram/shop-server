using CSharpFunctionalExtensions;
using Shop.Domain.Common;

namespace Shop.Domain.Manufacturers;

public interface IManufacturerRepository
{
    Task<Maybe<Manufacturer>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeManufacturerId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(Slug slug, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    void Add(Manufacturer manufacturer);
}