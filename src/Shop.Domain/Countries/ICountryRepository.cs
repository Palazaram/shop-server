using CSharpFunctionalExtensions;
using Shop.Domain.Common;

namespace Shop.Domain.Countries;

public interface ICountryRepository
{
    Task<Maybe<Country>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeCountryId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(
        Slug slug,
        Guid? excludeCountryId,
        CancellationToken cancellationToken = default);

    void Add(Country country);
}