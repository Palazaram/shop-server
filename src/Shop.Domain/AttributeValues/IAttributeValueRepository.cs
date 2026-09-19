using CSharpFunctionalExtensions;
using Shop.Domain.Common;

namespace Shop.Domain.AttributeValues;

public interface IAttributeValueRepository
{
    Task<Maybe<AttributeValue>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        Guid attributeId,
        string name,
        Guid? excludeValueId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(
        Guid attributeId,
        Slug slug,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AttributeValueRef>> GetRefsAsync(IReadOnlyCollection<Guid> valueIds, CancellationToken cancellationToken = default);

    void Add(AttributeValue value);
}