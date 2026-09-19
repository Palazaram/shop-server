using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Products;
using Shop.Domain.Errors;
using Shop.Domain.Products;

namespace Shop.Persistence.Queries;

internal sealed class ProductListQueries(AppDbContext context) : IProductListQueries
{
    private const string SortNewest = "newest";
    private const string SortPriceAsc = "price_asc";
    private const string SortPriceDesc = "price_desc";

    public async Task<Result<ProductListResponse, Error>> ListAsync(
        ProductListQuery query, CancellationToken cancellationToken)
    {
        string sort = string.IsNullOrWhiteSpace(query.Sort) ? SortNewest : query.Sort.Trim();

        if (sort is not (SortNewest or SortPriceAsc or SortPriceDesc))
            return DomainErrors.Products.UnknownSort(sort);

        // Поддерево: категория и её прямые дети. Опирается на ограничение глубины двумя уровнями.
        var nodes = await context.Categories.AsNoTracking()
            .Where(c => c.Id == query.CategoryId || c.ParentId == query.CategoryId)
            .Select(c => new { c.Id })
            .ToListAsync(cancellationToken);

        if (!nodes.Exists(n => n.Id == query.CategoryId))
            return DomainErrors.Categories.NotFound();

        List<Guid> subtreeIds = [.. nodes.Select(n => n.Id)];

        Result<List<List<Guid>>, Error> resolvedFilters =
            await ResolveFiltersAsync(query.Filters, cancellationToken);

        if (resolvedFilters.IsFailure)
            return resolvedFilters.Error;

        var rows = from variant in context.ProductVariants.AsNoTracking()
                   join product in context.Products on variant.ProductId equals product.Id
                   join manufacturer in context.Manufacturers
                       on product.ManufacturerId equals manufacturer.Id
                   where subtreeIds.Contains(product.CategoryId)
                   select new { variant, product, manufacturer };

        IQueryable<ProductAttributeValue> links = context.Set<ProductAttributeValue>();

        foreach (List<Guid> valueIds in resolvedFilters.Value)
        {
            rows = rows.Where(row => links.Any(
                link => link.ProductId == row.product.Id
                     && valueIds.Contains(link.AttributeValueId)));
        }

        int totalItems = await rows.CountAsync(cancellationToken);

        var ordered = sort switch
        {
            SortPriceAsc => rows.OrderBy(row => row.variant.Price)
                                .ThenBy(row => row.variant.Id),
            SortPriceDesc => rows.OrderByDescending(row => row.variant.Price)
                                 .ThenBy(row => row.variant.Id),
            _ => rows.OrderByDescending(row => row.variant.Id)
        };

        var page = await ordered
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(row => new
            {
                row.variant.Id,
                row.variant.Sku,
                row.variant.Slug,
                row.variant.Packaging,
                row.variant.Price,
                row.variant.StockQuantity,
                ProductId = row.product.Id,
                ProductName = row.product.Name,
                ManufacturerName = row.manufacturer.Name
            })
            .ToListAsync(cancellationToken);

        IReadOnlyList<ProductListItemResponse> items =
        [
            .. page.Select(row => new ProductListItemResponse(
                row.Id,
                row.ProductId,
                $"{row.ProductName} {row.Packaging}",
                row.Slug.Value,
                row.Sku,
                row.Price.Value,
                row.StockQuantity,
                row.ManufacturerName))
        ];

        int totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)query.PageSize);

        return new ProductListResponse(items, query.Page, query.PageSize, totalItems, totalPages);
    }

    private async Task<Result<List<List<Guid>>, Error>> ResolveFiltersAsync(
        IReadOnlyList<ProductListFilter> filters, CancellationToken cancellationToken)
    {
        List<List<Guid>> resolved = [];

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

            resolved.Add(valueIds);
        }

        return resolved;
    }
}