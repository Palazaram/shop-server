using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Options;
using Shop.Application.Products;
using Shop.Domain.Errors;
using Shop.Domain.Products;

namespace Shop.Persistence.Queries;

internal sealed class ProductListQueries(AppDbContext context, ImageUrlOptions imageUrls)
    : IProductListQueries
{
    private const string SortNewest = "newest";
    private const string SortPriceAsc = "price_asc";
    private const string SortPriceDesc = "price_desc";
    private const string SortNameAsc = "name_asc";
    private const string SortNameDesc = "name_desc";
    private const string SortRelevance = "relevance";

    public async Task<Result<ProductListResponse, Error>> ListAsync(ProductListQuery query, CancellationToken cancellationToken)
    {
        string? search = query.Filters.Search;
        bool hasSearch = !string.IsNullOrWhiteSpace(search);

        // При поиске порядок по умолчанию — релевантность: «новизна» в выдаче поиска
        // выглядит случайной.
        string sort = string.IsNullOrWhiteSpace(query.Sort)
            ? hasSearch ? SortRelevance : SortNewest
            : query.Sort.Trim();

        if (sort is not (SortNewest or SortPriceAsc or SortPriceDesc
                         or SortNameAsc or SortNameDesc or SortRelevance))
            return DomainErrors.Products.UnknownSort(sort);

        if (sort == SortRelevance && !hasSearch)
            return DomainErrors.Products.RelevanceSortRequiresSearch();

        // Поддерево: категория и её прямые дети. Опирается на ограничение глубины двумя уровнями.
        var nodes = await context.Categories.AsNoTracking()
            .Where(c => c.Id == query.CategoryId || c.ParentId == query.CategoryId)
            .Select(c => new { c.Id })
            .ToListAsync(cancellationToken);

        if (!nodes.Exists(n => n.Id == query.CategoryId))
            return DomainErrors.Categories.NotFound();

        List<Guid> subtreeIds = [.. nodes.Select(n => n.Id)];

        Result<List<ResolvedFilter>, Error> resolvedFilters = await ProductFilterResolver
            .ResolveAsync(context, query.Filters.Filters, cancellationToken);

        if (resolvedFilters.IsFailure)
            return resolvedFilters.Error;

        SearchCriteria? criteria = await ProductFilterResolver.ResolveSearchAsync(
            context, search, cancellationToken);

        var rows = from variant in ProductFilterResolver.MatchingVariants(
                       context, subtreeIds, resolvedFilters.Value,
                       query.Filters.PriceMin, query.Filters.PriceMax, criteria)
                   join product in context.Products on variant.ProductId equals product.Id
                   join manufacturer in context.Manufacturers
                       on product.ManufacturerId equals manufacturer.Id
                   select new { variant, product, manufacturer };

        int totalItems = await rows.CountAsync(cancellationToken);

        var ordered = sort switch
        {
            SortRelevance => rows.OrderByDescending(row =>
                                    EF.Functions.TrigramsWordSimilarity(search!, row.product.Name))
                                 .ThenBy(row => row.variant.Id),
            SortNameAsc => rows.OrderBy(row => EF.Functions.Collate(
                                    row.product.Name, TextComparers.UkrainianCollation))
                               .ThenBy(row => row.variant.Id),
            SortNameDesc => rows.OrderByDescending(row => EF.Functions.Collate(
                                     row.product.Name, TextComparers.UkrainianCollation))
                                .ThenBy(row => row.variant.Id),
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

        List<Guid> productIds = [.. page.Select(row => row.ProductId).Distinct()];

        // Обложка — первая по порядку. Один запрос на страницу, а не по запросу на товар.
        var covers = await context.Set<ProductImage>()
            .AsNoTracking()
            .Where(image => productIds.Contains(image.ProductId) && image.DisplayOrder == 0)
            .Select(image => new { image.ProductId, image.Id, image.Alt })
            .ToListAsync(cancellationToken);

        Dictionary<Guid, ProductImageThumbnail> coverByProduct = covers.ToDictionary(
            cover => cover.ProductId,
            cover => new ProductImageThumbnail(
                ProductImagePaths.Url(
                    imageUrls.PublicBaseUrl, cover.ProductId, cover.Id, ProductImagePaths.ThumbSize),
                ProductImagePaths.Url(
                    imageUrls.PublicBaseUrl, cover.ProductId, cover.Id, ProductImagePaths.CardSize),
                cover.Alt));

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
                row.ManufacturerName,
                coverByProduct.GetValueOrDefault(row.ProductId)))
        ];

        int totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)query.PageSize);

        return new ProductListResponse(items, query.Page, query.PageSize, totalItems, totalPages);
    }
}