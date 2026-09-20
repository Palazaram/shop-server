using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Abstractions;
using Shop.Application.Products;
using Shop.Domain.Errors;

namespace Shop.Persistence.Queries;

internal sealed class ProductListQueries(AppDbContext context, ISlugGenerator slugGenerator) : IProductListQueries
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

        Result<List<ResolvedFilter>, Error> resolvedFilters = await ProductFilterResolver
            .ResolveAsync(context, slugGenerator, query.Filters.Filters, cancellationToken);

        if (resolvedFilters.IsFailure)
            return resolvedFilters.Error;

        var rows = from variant in ProductFilterResolver.MatchingVariants(
                       context, subtreeIds, resolvedFilters.Value,
                       query.Filters.PriceMin, query.Filters.PriceMax)
                   join product in context.Products on variant.ProductId equals product.Id
                   join manufacturer in context.Manufacturers
                       on product.ManufacturerId equals manufacturer.Id
                   select new { variant, product, manufacturer };

        int totalItems = await rows.CountAsync(cancellationToken);

        var ordered = sort switch
        {
            SortPriceAsc => rows.OrderBy(row => row.variant.Price.Value)
                                .ThenBy(row => row.variant.Id),
            SortPriceDesc => rows.OrderByDescending(row => row.variant.Price.Value)
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
}