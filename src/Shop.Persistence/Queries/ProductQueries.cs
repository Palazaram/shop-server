using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Application.ProductAttributes;
using Shop.Application.Products;
using Shop.Domain.Errors;
using Shop.Domain.Products;

namespace Shop.Persistence.Queries;

internal sealed class ProductQueries(AppDbContext context) : IProductQueries
{
    private const string SortNewest = "newest";
    private const string SortNameAsc = "name_asc";
    private const string SortNameDesc = "name_desc";

    public async Task<Result<AdminProductListResponse, Error>> ListAsync(
        AdminProductListQuery query,
        CancellationToken cancellationToken)
    {
        string sort = string.IsNullOrWhiteSpace(query.Sort) ? SortNewest : query.Sort.Trim();

        if (sort is not (SortNewest or SortNameAsc or SortNameDesc))
            return DomainErrors.Products.UnknownSort(sort);

        List<ProductFilter> filters = [];

        if (query.CategorySlugs.Count > 0)
            filters.Add(new ProductFilter(ProductFilter.CategoryKey, query.CategorySlugs));

        if (query.ManufacturerSlugs.Count > 0)
            filters.Add(new ProductFilter(ProductFilter.ManufacturerKey, query.ManufacturerSlugs));

        // Разрешение слагов и поиск — общие с каталогом: и поддерево категории, и 400
        // на неизвестный слаг должны вести себя одинаково по обе стороны админки.
        Result<List<ResolvedFilter>, Error> resolved =
            await ProductFilterResolver.ResolveAsync(context, filters, cancellationToken);

        if (resolved.IsFailure)
            return resolved.Error;

        SearchCriteria? criteria = await ProductFilterResolver.ResolveSearchAsync(
            context, query.Search, cancellationToken);

        IQueryable<Product> products = context.Products.AsNoTracking();

        // Резолвер зовётся, только когда есть что разрешать: без фильтров и поиска условие
        // выродилось бы в «id товара есть среди id товаров» — полусоединение таблицы с самой
        // собой по первичному ключу, которое база обязана выполнить, потому что доказать его
        // истинность она не может.
        if (resolved.Value.Count > 0 || criteria is not null)
        {
            IQueryable<Guid> matchingIds = ProductFilterResolver.MatchingProductIds(
                context, null, resolved.Value, criteria);

            products = products.Where(p => matchingIds.Contains(p.Id));
        }

        if (query.IsFeatured is bool featured)
            products = products.Where(p => p.IsFeatured == featured);

        int totalItems = await products.CountAsync(cancellationToken);

        var rows = from product in products
                   join category in context.Categories on product.CategoryId equals category.Id
                   join manufacturer in context.Manufacturers
                       on product.ManufacturerId equals manufacturer.Id
                   select new { product, category, manufacturer };

        var ordered = sort switch
        {
            SortNameAsc => rows.OrderBy(row => EF.Functions.Collate(
                                   row.product.Name, TextComparers.UkrainianCollation))
                               .ThenBy(row => row.product.Id),
            SortNameDesc => rows.OrderByDescending(row => EF.Functions.Collate(
                                    row.product.Name, TextComparers.UkrainianCollation))
                                .ThenBy(row => row.product.Id),
            _ => rows.OrderByDescending(row => row.product.Id)
        };

        var page = await ordered
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(row => new
            {
                row.product.Id,
                row.product.Name,
                row.product.IsFeatured,
                CategoryName = row.category.Name,
                ManufacturerName = row.manufacturer.Name
            })
            .ToListAsync(cancellationToken);

        List<Guid> pageIds = [.. page.Select(row => row.Id)];

        // По запросу на страницу, а не подзапросом на строку: счётчик в списке нужен только
        // для видимых препаратов.
        Dictionary<Guid, int> variantCounts = await CountByProductAsync(
            context.ProductVariants.Select(v => v.ProductId), pageIds, cancellationToken);

        Dictionary<Guid, int> imageCounts = await CountByProductAsync(
            context.Set<ProductImage>().Select(image => image.ProductId), pageIds, cancellationToken);

        IReadOnlyList<AdminProductListItemResponse> items =
        [
            .. page.Select(row => new AdminProductListItemResponse(
                row.Id,
                row.Name,
                row.CategoryName,
                row.ManufacturerName,
                variantCounts.GetValueOrDefault(row.Id),
                imageCounts.GetValueOrDefault(row.Id),
                row.IsFeatured))
        ];

        int totalPages = totalItems == 0
            ? 0
            : (int)Math.Ceiling(totalItems / (double)query.PageSize);

        return new AdminProductListResponse(
            items, query.Page, query.PageSize, totalItems, totalPages);
    }

    private static async Task<Dictionary<Guid, int>> CountByProductAsync(
        IQueryable<Guid> productIds,
        List<Guid> pageIds,
        CancellationToken cancellationToken)
    {
        // Пустая страница — законный ответ фильтра, и спрашивать у базы счётчики
        // для пустого списка незачем.
        if (pageIds.Count == 0)
            return [];

        var counted = await productIds
            .Where(id => pageIds.Contains(id))
            .GroupBy(id => id)
            .Select(group => new { ProductId = group.Key, Count = group.Count() })
            .ToListAsync(cancellationToken);

        return counted.ToDictionary(row => row.ProductId, row => row.Count);
    }

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