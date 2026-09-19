using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Application.ProductAttributes;
using Shop.Application.Products;
using Shop.Domain.Products;

namespace Shop.Persistence.Queries;

internal sealed class ProductQueries(AppDbContext context) : IProductQueries
{
    public async Task<Maybe<IReadOnlyList<ProductAttributeValueGroupResponse>>>
        GetAttributeValuesAsync(Guid productId, CancellationToken cancellationToken)
    {
        bool exists = await context.Products
            .AnyAsync(p => p.Id == productId, cancellationToken);

        if (!exists)
            return Maybe<IReadOnlyList<ProductAttributeValueGroupResponse>>.None;

        var rows = await (
            from link in context.Set<ProductAttributeValue>().AsNoTracking()
            join value in context.AttributeValues on link.AttributeValueId equals value.Id
            join attribute in context.ProductAttributes on value.AttributeId equals attribute.Id
            where link.ProductId == productId
            select new
            {
                AttributeId = attribute.Id,
                AttributeName = attribute.Name,
                AttributeSlug = attribute.Slug,
                ValueId = value.Id,
                ValueName = value.Name,
                ValueSlug = value.Slug
            })
            .ToListAsync(cancellationToken);

        IReadOnlyList<ProductAttributeValueGroupResponse> groups = rows
            .GroupBy(row => row.AttributeId)
            .OrderBy(group => group.First().AttributeName, TextComparers.Ukrainian)
            .Select(group => new ProductAttributeValueGroupResponse(
                group.Key,
                group.First().AttributeName,
                group.First().AttributeSlug.Value,
                group
                    .OrderBy(row => row.ValueName, TextComparers.Ukrainian)
                    .Select(row => new AttributeValueResponse(
                        row.ValueId, row.ValueName, row.ValueSlug.Value))
                    .ToList()))
            .ToList();

        return Maybe<IReadOnlyList<ProductAttributeValueGroupResponse>>.From(groups);
    }
}