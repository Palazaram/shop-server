using Microsoft.EntityFrameworkCore;
using Shop.Application.ProductAttributes;

namespace Shop.Persistence.Queries;

internal sealed class ProductAttributeQueries(AppDbContext context) : IProductAttributeQueries
{
    public async Task<IReadOnlyList<ProductAttributeResponse>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var rows = await context.ProductAttributes
            .AsNoTracking()
            .Select(a => new
            {
                a.Id,
                a.Name,
                a.Slug,
                ValueCount = context.AttributeValues.Count(v => v.AttributeId == a.Id)
            })
            .ToListAsync(cancellationToken);

        return rows
            .OrderBy(row => row.Name, TextComparers.Ukrainian)
            .Select(row => new ProductAttributeResponse(
                row.Id, row.Name, row.Slug.Value, row.ValueCount))
            .ToList();
    }

    public async Task<IReadOnlyList<AttributeValueResponse>> GetValuesAsync(
        Guid attributeId,
        CancellationToken cancellationToken)
    {
        var rows = await context.AttributeValues
            .AsNoTracking()
            .Where(v => v.AttributeId == attributeId)
            .Select(v => new { v.Id, v.Name, v.Slug })
            .ToListAsync(cancellationToken);

        return rows
            .OrderBy(row => row.Name, TextComparers.Ukrainian)
            .Select(row => new AttributeValueResponse(row.Id, row.Name, row.Slug.Value))
            .ToList();
    }
}