using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Products;
using Shop.Domain.Categories;
using Shop.Domain.Errors;
using Shop.Domain.ProductVariants;
using Shop.Domain.Products;

namespace Shop.Persistence.Queries;

internal sealed class ProductFilterQueries(AppDbContext context) : IProductFilterQueries
{
    private const string ManufacturerGroupName = "Виробник";
    private const string CountryGroupName = "Країна походження";
    private const string PackagingGroupName = "Фасовка";

    public async Task<Result<ProductFiltersResponse, Error>> GetAsync(
        Guid? categoryId,
        ProductFilterSet filters,
        CancellationToken cancellationToken)
    {
        var categoryRows = await context.Categories
            .AsNoTracking()
            .Select(c => new
            {
                c.Id,
                c.Name,
                Slug = c.Slug.Value,
                c.ParentId,
                c.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        // null — весь каталог. Поддерево строится, только когда категория названа.
        List<Guid>? subtreeIds = null;
        Guid? parentId = null;

        if (categoryId is Guid requested)
        {
            var self = categoryRows.Find(row => row.Id == requested);

            if (self is null)
                return DomainErrors.Categories.NotFound();

            parentId = self.ParentId;

            subtreeIds =
            [
                self.Id,
                .. categoryRows.Where(row => row.ParentId == self.Id).Select(row => row.Id)
            ];
        }

        Result<List<ResolvedFilter>, Error> resolved = await ProductFilterResolver
            .ResolveAsync(context, filters.Filters, cancellationToken);

        if (resolved.IsFailure)
            return resolved.Error;

        List<ResolvedFilter> applied = resolved.Value;

        Dictionary<string, HashSet<string>> selected = filters.Filters.ToDictionary(
            f => f.GroupKey,
            f => new HashSet<string>(f.ValueKeys, StringComparer.Ordinal),
            StringComparer.Ordinal);

        IQueryable<ProductAttributeValue> assignments = context.Set<ProductAttributeValue>();

        SearchCriteria? searchCriteria = await ProductFilterResolver.ResolveSearchAsync(
            context, filters.Search, cancellationToken);

        IQueryable<ProductVariant> Matching(string? exceptGroupKey)
        {
            bool exceptPrice = exceptGroupKey == ProductFilterSet.PriceGroupKey;

            return ProductFilterResolver.MatchingVariants(
                context,
                subtreeIds,
                exceptGroupKey is null
                    ? applied
                    : applied.Where(f => f.GroupKey != exceptGroupKey),
                exceptPrice ? null : filters.PriceMin,
                exceptPrice ? null : filters.PriceMax,
                searchCriteria);
        }

        async Task<Dictionary<Guid, int>> CountByValueAsync(IQueryable<ProductVariant> variants)
        {
            var counted = await (
                from variant in variants
                join assignment in assignments on variant.ProductId equals assignment.ProductId
                group variant by assignment.AttributeValueId into grouped
                select new { Key = grouped.Key, Count = grouped.Count() })
                .ToListAsync(cancellationToken);

            return counted.ToDictionary(c => c.Key, c => c.Count);
        }

        async Task<Dictionary<Guid, int>> CountByManufacturerAsync(
            IQueryable<ProductVariant> variants)
        {
            var counted = await (
                from variant in variants
                join product in context.Products on variant.ProductId equals product.Id
                group variant by product.ManufacturerId into grouped
                select new { Key = grouped.Key, Count = grouped.Count() })
                .ToListAsync(cancellationToken);

            return counted.ToDictionary(c => c.Key, c => c.Count);
        }

        async Task<Dictionary<Guid, int>> CountByCategoryAsync(IQueryable<ProductVariant> variants)
        {
            var counted = await (
                from variant in variants
                join product in context.Products on variant.ProductId equals product.Id
                group variant by product.CategoryId into grouped
                select new { Key = grouped.Key, Count = grouped.Count() })
                .ToListAsync(cancellationToken);

            return counted.ToDictionary(c => c.Key, c => c.Count);
        }

        async Task<Dictionary<string, int>> CountByPackagingAsync(
            IQueryable<ProductVariant> variants)
        {
            var counted = await (
                from variant in variants
                group variant by variant.Packaging.Key into grouped
                select new { Key = grouped.Key, Count = grouped.Count() })
                .ToListAsync(cancellationToken);

            return counted.ToDictionary(c => c.Key, c => c.Count, StringComparer.Ordinal);
        }

        List<ProductFilterGroupResponse> groups = [];

        // --- Категории

        HashSet<string> chosenCategories =
            selected.GetValueOrDefault(ProductFilter.CategoryKey, []);

        Dictionary<Guid, int> categoryCounts = await CountByCategoryAsync(
            Matching(chosenCategories.Count == 0 ? null : ProductFilter.CategoryKey));

        // Товары лежат только в листьях, поэтому собственный счётчик корня почти всегда ноль,
        // а показать надо сумму по поддереву.
        ProductFilterCategoryResponse BuildCategoryNode(Guid nodeId, string slug, string name)
        {
            var childRows = categoryRows.Where(child => child.ParentId == nodeId).ToList();

            List<ProductFilterCategoryResponse> children =
            [
                .. childRows
                    .OrderBy(child => child.DisplayOrder)
                    .ThenBy(child => child.Name, TextComparers.Ukrainian)
                    .Select(child => new ProductFilterCategoryResponse(
                        child.Slug,
                        child.Name,
                        categoryCounts.GetValueOrDefault(child.Id),
                        []))
                    .Where(child => child.ProductCount > 0
                                    || chosenCategories.Contains(child.Key))
            ];

            int count = categoryCounts.GetValueOrDefault(nodeId)
                + childRows.Sum(child => categoryCounts.GetValueOrDefault(child.Id));

            return new ProductFilterCategoryResponse(slug, name, count, children);
        }

        List<ProductFilterCategoryResponse> categories =
        [
            .. categoryRows
                .Where(row => categoryId is Guid parent
                    ? row.ParentId == parent
                    : row.ParentId is null)
                .OrderBy(row => row.DisplayOrder)
                .ThenBy(row => row.Name, TextComparers.Ukrainian)
                .Select(row => BuildCategoryNode(row.Id, row.Slug, row.Name))
                .Where(node => node.ProductCount > 0
                               || node.Subcategories.Count > 0
                               || chosenCategories.Contains(node.Key))
        ];

        // --- Группы атрибутов: атрибуты принадлежат категории, в общем каталоге их нет.

        if (categoryId is Guid attributeScope)
        {
            List<Guid> owners = parentId is null
                ? [attributeScope]
                : [attributeScope, parentId.Value];

            var categoryLinks = await context.Set<CategoryAttribute>()
                .AsNoTracking()
                .Where(ca => owners.Contains(ca.CategoryId))
                .Select(ca => new { ca.CategoryId, ca.AttributeId, ca.DisplayOrder })
                .ToListAsync(cancellationToken);

            bool hasOwn = categoryLinks.Exists(l => l.CategoryId == attributeScope);
            Guid owner = CategoryAttributeInheritance.ResolveOwner(
                attributeScope, parentId, hasOwn);

            var effective = categoryLinks
                .Where(l => l.CategoryId == owner)
                .OrderBy(l => l.DisplayOrder)
                .ToList();

            if (effective.Count > 0)
            {
                HashSet<Guid> attributeIds = [.. effective.Select(l => l.AttributeId)];

                var values = await (
                    from value in context.AttributeValues
                    join attribute in context.ProductAttributes
                        on value.AttributeId equals attribute.Id
                    where attributeIds.Contains(value.AttributeId)
                    select new
                    {
                        value.Id,
                        value.AttributeId,
                        value.Name,
                        value.Slug,
                        AttributeName = attribute.Name,
                        AttributeSlug = attribute.Slug
                    })
                    .AsNoTracking()
                    .ToListAsync(cancellationToken);

                var byAttribute = values
                    .GroupBy(v => v.AttributeId)
                    .ToDictionary(g => g.Key, g => g.ToList());

                Dictionary<Guid, int> baseCounts = await CountByValueAsync(Matching(null));

                foreach (var link in effective)
                {
                    if (!byAttribute.TryGetValue(link.AttributeId, out var groupValues))
                        continue;

                    string groupKey =
                        ProductFilter.AttributePrefix + groupValues[0].AttributeSlug.Value;

                    HashSet<string> chosen = selected.GetValueOrDefault(groupKey, []);

                    Dictionary<Guid, int> counts = chosen.Count == 0
                        ? baseCounts
                        : await CountByValueAsync(Matching(groupKey));

                    var visible = groupValues
                        .Select(value => new
                        {
                            Key = value.Slug.Value,
                            value.Name,
                            Count = counts.GetValueOrDefault(value.Id)
                        })
                        .Where(entry => entry.Count > 0 || chosen.Contains(entry.Key))
                        .OrderBy(entry => entry.Name, TextComparers.Ukrainian)
                        .ToList();

                    if (visible.Count == 0)
                        continue;

                    groups.Add(new ProductFilterGroupResponse(
                        groupKey,
                        groupValues[0].AttributeName,
                        [.. visible.Select(entry => new ProductFilterValueResponse(
                            entry.Key, entry.Name, entry.Count))]));
                }
            }
        }

        // --- Производитель и страна

        List<ManufacturerRef> manufacturers = await ProductFilterResolver
            .LoadManufacturersAsync(context, cancellationToken);

        List<CountryRef> countries = await ProductFilterResolver
            .LoadCountriesAsync(context, cancellationToken);

        HashSet<string> chosenManufacturers =
            selected.GetValueOrDefault(ProductFilter.ManufacturerKey, []);

        HashSet<string> chosenCountries =
            selected.GetValueOrDefault(ProductFilter.CountryKey, []);

        Dictionary<Guid, int> baseManufacturerCounts =
            await CountByManufacturerAsync(Matching(null));

        Dictionary<Guid, int> manufacturerCounts = chosenManufacturers.Count == 0
            ? baseManufacturerCounts
            : await CountByManufacturerAsync(Matching(ProductFilter.ManufacturerKey));

        var visibleManufacturers = manufacturers
            .Select(m => new { m.Slug, m.Name, Count = manufacturerCounts.GetValueOrDefault(m.Id) })
            .Where(entry => entry.Count > 0 || chosenManufacturers.Contains(entry.Slug))
            .OrderBy(entry => entry.Name, TextComparers.Ukrainian)
            .ToList();

        if (visibleManufacturers.Count > 0)
        {
            groups.Add(new ProductFilterGroupResponse(
                ProductFilter.ManufacturerKey,
                ManufacturerGroupName,
                [.. visibleManufacturers.Select(entry => new ProductFilterValueResponse(
                    entry.Slug, entry.Name, entry.Count))]));
        }

        Dictionary<Guid, int> countryCounts = chosenCountries.Count == 0
            ? baseManufacturerCounts
            : await CountByManufacturerAsync(Matching(ProductFilter.CountryKey));

        var visibleCountries = countries
            .Select(country => new
            {
                Key = country.Slug,
                country.Name,
                Count = manufacturers
                    .Where(m => m.CountryId == country.Id)
                    .Sum(m => countryCounts.GetValueOrDefault(m.Id))
            })
            .Where(entry => entry.Count > 0 || chosenCountries.Contains(entry.Key))
            .OrderBy(entry => entry.Name, TextComparers.Ukrainian)
            .ToList();

        if (visibleCountries.Count > 0)
        {
            groups.Add(new ProductFilterGroupResponse(
                ProductFilter.CountryKey,
                CountryGroupName,
                [.. visibleCountries.Select(entry => new ProductFilterValueResponse(
                    entry.Key, entry.Name, entry.Count))]));
        }

        // --- Фасовка

        HashSet<string> chosenPackagings =
            selected.GetValueOrDefault(ProductFilter.PackagingKey, []);

        var packagingRows = await ProductFilterResolver
            .MatchingVariants(context, subtreeIds, [], null, null)
            .Select(v => new { v.Packaging.Key, v.Packaging.Value, v.Packaging.Unit })
            .Distinct()
            .ToListAsync(cancellationToken);

        Dictionary<string, int> packagingCounts = chosenPackagings.Count == 0
            ? await CountByPackagingAsync(Matching(null))
            : await CountByPackagingAsync(Matching(ProductFilter.PackagingKey));

        var visiblePackagings = packagingRows
            .Select(row => new
            {
                row.Key,
                row.Unit,
                row.Value,
                Name = Packaging.Create(row.Value, row.Unit).Value.ToString(),
                Count = packagingCounts.GetValueOrDefault(row.Key)
            })
            .Where(entry => entry.Count > 0 || chosenPackagings.Contains(entry.Key))
            .OrderBy(entry => entry.Unit)
            .ThenBy(entry => entry.Value)
            .ToList();

        if (visiblePackagings.Count > 0)
        {
            groups.Add(new ProductFilterGroupResponse(
                ProductFilter.PackagingKey,
                PackagingGroupName,
                [.. visiblePackagings.Select(entry => new ProductFilterValueResponse(
                    entry.Key, entry.Name, entry.Count))]));
        }

        // --- Диапазон цены

        IQueryable<ProductVariant> priceScope = Matching(ProductFilterSet.PriceGroupKey);

        decimal? minPrice = await priceScope.MinAsync(v => (decimal?)v.Price.Value, cancellationToken);
        decimal? maxPrice = await priceScope.MaxAsync(v => (decimal?)v.Price.Value, cancellationToken);

        PriceRangeResponse? priceRange = minPrice is decimal low && maxPrice is decimal high
            ? new PriceRangeResponse(low, high)
            : null;

        return new ProductFiltersResponse(categories, groups, priceRange);
    }
}
