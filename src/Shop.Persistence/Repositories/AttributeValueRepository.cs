using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Domain.AttributeValues;
using Shop.Domain.Common;

namespace Shop.Persistence.Repositories;

internal sealed class AttributeValueRepository(AppDbContext context) : IAttributeValueRepository
{
    public async Task<Maybe<AttributeValue>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.AttributeValues.FindAsync([id], cancellationToken);

    public Task<bool> ExistsByNameAsync(
        Guid attributeId,
        string name,
        Guid? excludeValueId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<AttributeValue> query = context.AttributeValues
            .Where(v => v.AttributeId == attributeId && v.Name == name);

        if (excludeValueId.HasValue)
            query = query.Where(v => v.Id != excludeValueId.Value);

        return query.AnyAsync(cancellationToken);
    }

    public Task<bool> ExistsBySlugAsync(Guid attributeId, Slug slug, CancellationToken cancellationToken = default)
        => context.AttributeValues.AnyAsync(
            v => v.AttributeId == attributeId && v.Slug == slug,
            cancellationToken);

    public void Add(AttributeValue value) => context.AttributeValues.Add(value);
}