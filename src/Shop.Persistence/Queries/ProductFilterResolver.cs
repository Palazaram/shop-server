using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Abstractions;
using Shop.Application.Products;
using Shop.Domain.Errors;
using Shop.Domain.Products;

namespace Shop.Persistence.Queries;

internal enum ResolvedFilterKind
{
    AttributeValues = 1,
    Manufacturers = 2
}

internal sealed record ResolvedFilter(string GroupKey, ResolvedFilterKind Kind, List<Guid> Ids);

internal sealed record ManufacturerRef(
    Guid Id, string Slug, string Name, string Country, string CountryKey);

internal static class ProductFilterResolver
{
    public static async Task<List<ManufacturerRef>> LoadManufacturersAsync(
        AppDbContext context,
        ISlugGenerator slugGenerator,
        CancellationToken cancellationToken)
    {
        var rows = await context.Manufacturers
            .AsNoTracking()
            .Select(m => new { m.Id, m.Slug, m.Name, m.Country })
            .ToListAsync(cancellationToken);

        return
        [
            .. rows.Select(m => new ManufacturerRef(
                m.Id, m.Slug.Value, m.Name, m.Country, slugGenerator.Generate(m.Country)))
        ];
    }

    public static async Task<Result<List<ResolvedFilter>, Error>> ResolveAsync(
        AppDbContext context,
        ISlugGenerator slugGenerator,
        IReadOnlyList<ProductFilter> filters,
        CancellationToken cancellationToken)
    {
        List<ResolvedFilter> resolved = [];

        if (filters.Count == 0)
            return resolved;

        bool needsAttributes = filters.Any(f => IsAttribute(f.GroupKey));
        bool needsManufacturers = filters.Any(f =>
            f.GroupKey is ProductFilter.ManufacturerKey or ProductFilter.CountryKey);

        Dictionary<string, Guid> attributeBySlug = [];
        Dictionary<(Guid, string), Guid> valueBySlug = [];

        if (needsAttributes)
        {
            var attributes = await context.ProductAttributes
                .AsNoTracking()
                .Select(a => new { a.Id, a.Slug })
                .ToListAsync(cancellationToken);

            attributeBySlug = attributes
                .ToDictionary(a => a.Slug.Value, a => a.Id, StringComparer.Ordinal);

            List<Guid> attributeIds = [];

            foreach (ProductFilter filter in filters)
            {
                if (!IsAttribute(filter.GroupKey))
                    continue;

                string slug = AttributeSlug(filter.GroupKey);

                if (!attributeBySlug.TryGetValue(slug, out Guid attributeId))
                    return DomainErrors.Products.UnknownFilter(slug);

                attributeIds.Add(attributeId);
            }

            var values = await context.AttributeValues
                .AsNoTracking()
                .Where(v => attributeIds.Contains(v.AttributeId))
                .Select(v => new { v.Id, v.AttributeId, v.Slug })
                .ToListAsync(cancellationToken);

            valueBySlug = values.ToDictionary(v => (v.AttributeId, v.Slug.Value), v => v.Id);
        }

        List<ManufacturerRef> manufacturers = needsManufacturers
            ? await LoadManufacturersAsync(context, slugGenerator, cancellationToken)
            : [];

        foreach (ProductFilter filter in filters)
        {
            if (IsAttribute(filter.GroupKey))
            {
                Guid attributeId = attributeBySlug[AttributeSlug(filter.GroupKey)];

                List<Guid> valueIds = [];

                foreach (string valueKey in filter.ValueKeys)
                {
                    if (!valueBySlug.TryGetValue((attributeId, valueKey), out Guid valueId))
                        return DomainErrors.Products.UnknownFilterValue(filter.GroupKey, valueKey);

                    valueIds.Add(valueId);
                }

                resolved.Add(new ResolvedFilter(
                    filter.GroupKey, ResolvedFilterKind.AttributeValues, valueIds));
                continue;
            }

            if (filter.GroupKey == ProductFilter.ManufacturerKey)
            {
                List<Guid> ids = [];

                foreach (string slug in filter.ValueKeys)
                {
                    ManufacturerRef? found = manufacturers.Find(m => m.Slug == slug);

                    if (found is null)
                        return DomainErrors.Products.UnknownFilterValue(filter.GroupKey, slug);

                    ids.Add(found.Id);
                }

                resolved.Add(new ResolvedFilter(
                    filter.GroupKey, ResolvedFilterKind.Manufacturers, ids));
                continue;
            }

            if (filter.GroupKey == ProductFilter.CountryKey)
            {
                List<Guid> ids = [];

                foreach (string countryKey in filter.ValueKeys)
                {
                    List<Guid> matched =
                    [
                        .. manufacturers
                            .Where(m => m.CountryKey.Length > 0 && m.CountryKey == countryKey)
                            .Select(m => m.Id)
                    ];

                    if (matched.Count == 0)
                        return DomainErrors.Products.UnknownFilterValue(filter.GroupKey, countryKey);

                    ids.AddRange(matched);
                }

                resolved.Add(new ResolvedFilter(
                    filter.GroupKey, ResolvedFilterKind.Manufacturers, ids));
                continue;
            }

            return DomainErrors.Products.UnknownFilter(filter.GroupKey);
        }

        return resolved;
    }

    public static IQueryable<Guid> MatchingProductIds(
        AppDbContext context,
        List<Guid> subtreeIds,
        IEnumerable<ResolvedFilter> filters)
    {
        IQueryable<ProductAttributeValue> assignments = context.Set<ProductAttributeValue>();

        IQueryable<Product> products = context.Products
            .AsNoTracking()
            .Where(p => subtreeIds.Contains(p.CategoryId));

        foreach (ResolvedFilter filter in filters)
        {
            List<Guid> ids = filter.Ids;

            products = filter.Kind == ResolvedFilterKind.AttributeValues
                ? products.Where(p => assignments.Any(
                    a => a.ProductId == p.Id && ids.Contains(a.AttributeValueId)))
                : products.Where(p => ids.Contains(p.ManufacturerId));
        }

        return products.Select(p => p.Id);
    }

    private static bool IsAttribute(string groupKey)
        => groupKey.StartsWith(ProductFilter.AttributePrefix, StringComparison.Ordinal)
        && groupKey.Length > ProductFilter.AttributePrefix.Length;

    private static string AttributeSlug(string groupKey)
        => groupKey[ProductFilter.AttributePrefix.Length..];
}