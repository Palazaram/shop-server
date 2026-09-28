using CSharpFunctionalExtensions;

namespace Shop.Domain.Specifications;

public interface ISpecificationRepository
{
    Task<Maybe<Specification>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Specification>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeSpecificationId,
        CancellationToken cancellationToken = default);

    Task<bool> AllExistAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);

    Task<int> GetNextDisplayOrderAsync(CancellationToken cancellationToken = default);

    void Add(Specification specification);
}
