using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Products;
using Shop.Domain.Errors;
using Shop.Domain.ProductVariants;
using Shop.Domain.Products;

namespace Shop.Persistence.Queries;

internal enum ResolvedFilterKind
{
    AttributeValues = 1,
    Manufacturers = 2,
    Packagings = 3,
    Categories = 4
}

/// <summary>
/// Разрешённый поиск: текст для оператора похожести, готовый шаблон LIKE и заранее найденные
/// производители. Список id вместо подзапроса — потому что коррелированный EXISTS нельзя
/// собрать в BitmapOr, и тогда ни одна ветка поиска не идёт через индекс.
/// </summary>
internal sealed record SearchCriteria(
    string Text,
    string Pattern,
    IReadOnlyList<Guid> ManufacturerIds);

internal sealed record ResolvedFilter(
    string GroupKey,
    ResolvedFilterKind Kind,
    List<Guid> Ids,
    List<string> Keys)
{
    public static ResolvedFilter ByIds(string groupKey, ResolvedFilterKind kind, List<Guid> ids)
        => new(groupKey, kind, ids, []);

    public static ResolvedFilter ByKeys(string groupKey, List<string> keys)
        => new(groupKey, ResolvedFilterKind.Packagings, [], keys);
}

internal sealed record ManufacturerRef(Guid Id, string Slug, string Name, Guid CountryId);

internal sealed record CountryRef(Guid Id, string Slug, string Name);
internal sealed record CategoryRef(Guid Id, string Slug, Guid? ParentId);

internal static class ProductFilterResolver
{
    /// <summary>
    /// Npgsql переводит ILike без третьего аргумента в ESCAPE '' — при таком escape обратная
    /// черта перестаёт экранировать, и «20%» в названии не найдётся никогда.
    /// </summary>
    private const string LikeEscape = "\\";

    public static async Task<List<ManufacturerRef>> LoadManufacturersAsync(
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var rows = await context.Manufacturers
            .AsNoTracking()
            .Select(m => new { m.Id, m.Slug, m.Name, m.CountryId })
            .ToListAsync(cancellationToken);

        return
        [
            .. rows.Select(row => new ManufacturerRef(
                row.Id, row.Slug.Value, row.Name, row.CountryId))
        ];
    }

    public static async Task<List<CountryRef>> LoadCountriesAsync(
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var rows = await context.Countries
            .AsNoTracking()
            .Select(c => new { c.Id, c.Slug, c.Name })
            .ToListAsync(cancellationToken);

        return [.. rows.Select(row => new CountryRef(row.Id, row.Slug.Value, row.Name))];
    }

    public static async Task<List<CategoryRef>> LoadCategoriesAsync(
        AppDbContext context,
        CancellationToken cancellationToken)
    {
        var rows = await context.Categories
            .AsNoTracking()
            .Select(c => new { c.Id, c.Slug, c.ParentId })
            .ToListAsync(cancellationToken);

        return [.. rows.Select(row => new CategoryRef(row.Id, row.Slug.Value, row.ParentId))];
    }

    public static async Task<Result<List<ResolvedFilter>, Error>> ResolveAsync(
        AppDbContext context,
        IReadOnlyList<ProductFilter> filters,
        CancellationToken cancellationToken)
    {
        List<ResolvedFilter> resolved = [];

        if (filters.Count == 0)
            return resolved;

        bool needsAttributes = filters.Any(f => IsAttribute(f.GroupKey));
        bool needsManufacturers = filters.Any(f =>
            f.GroupKey is ProductFilter.ManufacturerKey or ProductFilter.CountryKey);
        bool needsPackagings = filters.Any(f => f.GroupKey == ProductFilter.PackagingKey);

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
           ? await LoadManufacturersAsync(context, cancellationToken)
           : [];

        bool needsCountries = filters.Any(f => f.GroupKey == ProductFilter.CountryKey);

        List<CountryRef> countries = needsCountries
            ? await LoadCountriesAsync(context, cancellationToken)
            : [];

        bool needsCategories = filters.Any(f => f.GroupKey == ProductFilter.CategoryKey);

        List<CategoryRef> categories = needsCategories
            ? await LoadCategoriesAsync(context, cancellationToken)
            : [];

        HashSet<string> knownPackagings = [];

        if (needsPackagings)
        {
            List<string> keys = await context.ProductVariants
                .AsNoTracking()
                .Select(v => v.Packaging.Key)
                .Distinct()
                .ToListAsync(cancellationToken);

            knownPackagings = [.. keys];
        }

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

                resolved.Add(ResolvedFilter.ByIds(
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

                resolved.Add(ResolvedFilter.ByIds(
                    filter.GroupKey, ResolvedFilterKind.Manufacturers, ids));
                continue;
            }

            if (filter.GroupKey == ProductFilter.CountryKey)
            {
                List<Guid> ids = [];

                foreach (string countrySlug in filter.ValueKeys)
                {
                    CountryRef? country = countries.Find(c => c.Slug == countrySlug);

                    // Существование проверяем по справочнику, а не по наличию производителей.
                    if (country is null)
                        return DomainErrors.Products.UnknownFilterValue(filter.GroupKey, countrySlug);

                    ids.AddRange(manufacturers
                        .Where(m => m.CountryId == country.Id)
                        .Select(m => m.Id));
                }

                resolved.Add(ResolvedFilter.ByIds(
                    filter.GroupKey, ResolvedFilterKind.Manufacturers, ids));
                continue;
            }

            if (filter.GroupKey == ProductFilter.PackagingKey)
            {
                List<string> keys = [];

                foreach (string key in filter.ValueKeys)
                {
                    if (!knownPackagings.Contains(key))
                        return DomainErrors.Products.UnknownFilterValue(filter.GroupKey, key);

                    keys.Add(key);
                }

                resolved.Add(ResolvedFilter.ByKeys(filter.GroupKey, keys));
                continue;
            }

            if (filter.GroupKey == ProductFilter.CategoryKey)
            {
                List<Guid> ids = [];

                foreach (string categorySlug in filter.ValueKeys)
                {
                    CategoryRef? category = categories.Find(c => c.Slug == categorySlug);

                    if (category is null)
                        return DomainErrors.Products.UnknownFilterValue(filter.GroupKey, categorySlug);

                    // Выбор раздела включает его подразделы: товары лежат в листьях,
                    // и «Засоби захисту рослин» без детей вернул бы пусто.
                    ids.Add(category.Id);
                    ids.AddRange(categories
                        .Where(child => child.ParentId == category.Id)
                        .Select(child => child.Id));
                }

                resolved.Add(ResolvedFilter.ByIds(
                    filter.GroupKey, ResolvedFilterKind.Categories, ids));
                continue;
            }

            return DomainErrors.Products.UnknownFilter(filter.GroupKey);
        }

        return resolved;
    }

    public static IQueryable<ProductVariant> MatchingVariants(
        AppDbContext context,
        IReadOnlyList<Guid>? subtreeIds,
        IEnumerable<ResolvedFilter> filters,
        decimal? priceMin,
        decimal? priceMax,
        SearchCriteria? search = null)
    {
        List<ResolvedFilter> all = [.. filters];

        IQueryable<Guid> productIds = MatchingProductIds(
            context, subtreeIds, all.Where(f => f.Kind != ResolvedFilterKind.Packagings), search);

        IQueryable<ProductVariant> variants = context.ProductVariants
            .AsNoTracking()
            .Where(v => productIds.Contains(v.ProductId));

        foreach (ResolvedFilter filter in all.Where(f => f.Kind == ResolvedFilterKind.Packagings))
        {
            List<string> keys = filter.Keys;

            variants = variants.Where(v => keys.Contains(v.Packaging.Key));
        }

        if (priceMin is decimal min)
            variants = variants.Where(v => v.Price.Value >= min);

        if (priceMax is decimal max)
            variants = variants.Where(v => v.Price.Value <= max);

        return variants;
    }

    /// <summary>
    /// Товары под набор фильтров: тем же кодом пользуется админский список, которому фасовки
    /// не нужны вовсе.
    /// </summary>
    public static IQueryable<Guid> MatchingProductIds(
        AppDbContext context,
        IReadOnlyList<Guid>? subtreeIds,
        IEnumerable<ResolvedFilter> filters,
        SearchCriteria? search)
    {
        IQueryable<ProductAttributeValue> assignments = context.Set<ProductAttributeValue>();

        IQueryable<Product> products = context.Products.AsNoTracking();

        // null — это весь каталог: листинг вне категории.
        if (subtreeIds is not null)
        {
            List<Guid> categoryIds = [.. subtreeIds];
            products = products.Where(p => categoryIds.Contains(p.CategoryId));
        }

        foreach (ResolvedFilter filter in filters)
        {
            List<Guid> ids = filter.Ids;

            products = filter.Kind switch
            {
                ResolvedFilterKind.AttributeValues => products.Where(p => assignments.Any(
                    a => a.ProductId == p.Id && ids.Contains(a.AttributeValueId))),
                ResolvedFilterKind.Categories => products.Where(p => ids.Contains(p.CategoryId)),
                _ => products.Where(p => ids.Contains(p.ManufacturerId))
            };
        }

        if (search is not null)
        {
            List<Guid> manufacturerIds = [.. search.ManufacturerIds];

            products = products.Where(p =>
                EF.Functions.ILike(p.Name, search.Pattern, LikeEscape)
                || EF.Functions.TrigramsAreWordSimilar(search.Text, p.Name)
                || manufacturerIds.Contains(p.ManufacturerId));
        }

        return products.Select(p => p.Id);
    }

    public static async Task<SearchCriteria?> ResolveSearchAsync(
        AppDbContext context,
        string? search,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(search))
            return null;

        string pattern = $"%{EscapeLikePattern(search)}%";

        List<Guid> manufacturerIds = await context.Manufacturers
            .AsNoTracking()
            .Where(m => EF.Functions.ILike(m.Name, pattern, LikeEscape))
            .Select(m => m.Id)
            .ToListAsync(cancellationToken);

        return new SearchCriteria(search, pattern, manufacturerIds);
    }

    /// <summary>
    /// Без этого запрос «%» вернул бы весь каталог, а «_» совпадал бы с любым символом:
    /// спецсимволы LIKE пришли от пользователя и должны искаться буквально.
    /// </summary>
    private static string EscapeLikePattern(string value)
        => value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);

    private static bool IsAttribute(string groupKey)
        => groupKey.StartsWith(ProductFilter.AttributePrefix, StringComparison.Ordinal)
        && groupKey.Length > ProductFilter.AttributePrefix.Length;

    private static string AttributeSlug(string groupKey)
        => groupKey[ProductFilter.AttributePrefix.Length..];
}