using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Products;
using Shop.Domain.Errors;

namespace Shop.Persistence.Queries;

internal sealed record ResolvedAttributeFilter(Guid AttributeId, List<Guid> ValueIds);

internal static class AttributeFilterResolver
{
    public static async Task<Result<List<ResolvedAttributeFilter>, Error>> ResolveAsync(
        AppDbContext context,
        IReadOnlyList<ProductListFilter> filters,
        CancellationToken cancellationToken)
    {
        List<ResolvedAttributeFilter> resolved = [];

        if (filters.Count == 0)
            return resolved;

        var attributes = await context.ProductAttributes.AsNoTracking()
            .Select(a => new { a.Id, a.Slug })
            .ToListAsync(cancellationToken);

        Dictionary<string, Guid> attributeBySlug =
            attributes.ToDictionary(a => a.Slug.Value, a => a.Id, StringComparer.Ordinal);

        List<Guid> attributeIds = [];

        foreach (ProductListFilter filter in filters)
        {
            if (!attributeBySlug.TryGetValue(filter.AttributeSlug, out Guid attributeId))
                return DomainErrors.Products.UnknownFilter(filter.AttributeSlug);

            attributeIds.Add(attributeId);
        }

        var values = await context.AttributeValues.AsNoTracking()
            .Where(v => attributeIds.Contains(v.AttributeId))
            .Select(v => new { v.Id, v.AttributeId, v.Slug })
            .ToListAsync(cancellationToken);

        Dictionary<(Guid, string), Guid> valueBySlug =
            values.ToDictionary(v => (v.AttributeId, v.Slug.Value), v => v.Id);

        for (int index = 0; index < filters.Count; index++)
        {
            ProductListFilter filter = filters[index];
            Guid attributeId = attributeIds[index];

            List<Guid> valueIds = [];

            foreach (string valueSlug in filter.ValueSlugs)
            {
                if (!valueBySlug.TryGetValue((attributeId, valueSlug), out Guid valueId))
                    return DomainErrors.Products
                        .UnknownFilterValue(filter.AttributeSlug, valueSlug);

                valueIds.Add(valueId);
            }

            resolved.Add(new ResolvedAttributeFilter(attributeId, valueIds));
        }

        return resolved;
    }
}