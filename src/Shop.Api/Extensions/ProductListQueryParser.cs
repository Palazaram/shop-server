using CSharpFunctionalExtensions;
using Microsoft.Extensions.Primitives;
using Shop.Application.Products;
using Shop.Domain.Errors;
using System.Globalization;

namespace Shop.Api.Extensions;

/// <summary>
/// Разбор параметров листинга. Общий для листинга категории и общего каталога: два разбора
/// разошлись бы на первом же новом фильтре.
/// </summary>
internal static class ProductListQueryParser
{
    private const NumberStyles PriceStyles =
        NumberStyles.AllowLeadingWhite
        | NumberStyles.AllowTrailingWhite
        | NumberStyles.AllowLeadingSign
        | NumberStyles.AllowDecimalPoint;

    public static Result<ProductFilterSet, Error> ParseFilters(
        Guid? categoryId, IQueryCollection source)
    {
        List<ProductFilter> filters = [];

        foreach (KeyValuePair<string, StringValues> pair in source)
        {
            bool isAttribute =
                pair.Key.StartsWith(ProductFilter.AttributePrefix, StringComparison.Ordinal)
                && pair.Key.Length > ProductFilter.AttributePrefix.Length;

            bool isFixed = pair.Key is ProductFilter.ManufacturerKey
                                   or ProductFilter.CountryKey
                                   or ProductFilter.PackagingKey
                                   or ProductFilter.CategoryKey;

            if (!isAttribute && !isFixed)
                continue;

            string[] valueKeys = SplitValues(pair.Value);

            if (valueKeys.Length == 0)
                continue;

            // Атрибуты принадлежат категории: вне категории их не показать в сайдбаре
            // и не снять оттуда. Применить молча — значит оставить фильтр,
            // существующий только в адресной строке.
            if (isAttribute && categoryId is null)
                return DomainErrors.Products.AttributeFilterOutsideCategory(pair.Key);

            filters.Add(new ProductFilter(pair.Key, valueKeys));
        }

        (decimal? Value, string? Invalid) min = ReadPrice(source, ProductFilterSet.PriceMinKey);

        if (min.Invalid is not null)
            return DomainErrors.Products.InvalidPrice(ProductFilterSet.PriceMinKey, min.Invalid);

        (decimal? Value, string? Invalid) max = ReadPrice(source, ProductFilterSet.PriceMaxKey);

        if (max.Invalid is not null)
            return DomainErrors.Products.InvalidPrice(ProductFilterSet.PriceMaxKey, max.Invalid);

        string search = source[ProductFilterSet.SearchKey].ToString().Trim();

        if (search.Length == 0)
            return new ProductFilterSet(filters, min.Value, max.Value);

        if (search.Length < ProductFilterSet.MinSearchLength)
            return DomainErrors.Products.SearchTooShort(ProductFilterSet.MinSearchLength);

        if (search.Length > ProductFilterSet.MaxSearchLength)
            return DomainErrors.Products.SearchTooLong(ProductFilterSet.MaxSearchLength);

        return new ProductFilterSet(filters, min.Value, max.Value, search);
    }

    public static Result<ProductListQuery, Error> Build(Guid? categoryId, IQueryCollection source)
    {
        Result<ProductFilterSet, Error> filters = ParseFilters(categoryId, source);

        if (filters.IsFailure)
            return filters.Error;

        return new ProductListQuery(
            categoryId,
            filters.Value,
            source["sort"],
            ReadPage(source),
            ReadPageSize(source, ProductListQuery.DefaultPageSize, ProductListQuery.MaxPageSize));
    }

    /// <summary>
    /// Разбор админского списка. Общий <see cref="ParseFilters"/> здесь не годится: в нём живут
    /// фасовка и цена, а список показывает препараты — такой фильтр было бы нечем применить,
    /// и он молча пропал бы.
    /// </summary>
    public static Result<AdminProductListQuery, Error> BuildAdmin(IQueryCollection source)
    {
        string search = source[ProductFilterSet.SearchKey].ToString().Trim();

        if (search.Length > 0 && search.Length < ProductFilterSet.MinSearchLength)
            return DomainErrors.Products.SearchTooShort(ProductFilterSet.MinSearchLength);

        if (search.Length > ProductFilterSet.MaxSearchLength)
            return DomainErrors.Products.SearchTooLong(ProductFilterSet.MaxSearchLength);

        bool? isFeatured = null;
        string featured = source[AdminProductListQuery.FeaturedKey].ToString().Trim();

        if (featured.Length > 0)
        {
            if (!bool.TryParse(featured, out bool parsedFeatured))
                return DomainErrors.Products.InvalidFeaturedFilter(featured);

            isFeatured = parsedFeatured;
        }

        return new AdminProductListQuery(
            search.Length == 0 ? null : search,
            SplitValues(source[ProductFilter.CategoryKey]),
            SplitValues(source[ProductFilter.ManufacturerKey]),
            isFeatured,
            source["sort"],
            ReadPage(source),
            ReadPageSize(
                source, AdminProductListQuery.DefaultPageSize, AdminProductListQuery.MaxPageSize));
    }

    /// <summary>
    /// Зажимается так же, как page и pageSize: длина ответа сразу показывает, что применилось.
    /// </summary>
    public static int ReadLimit(IQueryCollection source, int defaultLimit, int maxLimit)
        => long.TryParse(source["limit"], NumberStyles.Integer, CultureInfo.InvariantCulture,
            out long parsed)
            ? (int)Math.Clamp(parsed, 1, maxLimit)
            : defaultLimit;

    private static string[] SplitValues(StringValues raw)
        => [.. raw
            .SelectMany(value => (value ?? string.Empty).Split(
                ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.Ordinal)];

    private static int ReadPage(IQueryCollection source)
        => int.TryParse(source["page"], NumberStyles.Integer, CultureInfo.InvariantCulture,
            out int parsed) && parsed > 0
            ? parsed
            : 1;

    private static int ReadPageSize(IQueryCollection source, int defaultSize, int maxSize)
        => int.TryParse(source["pageSize"], NumberStyles.Integer, CultureInfo.InvariantCulture,
            out int parsed)
            ? Math.Clamp(parsed, 1, maxSize)
            : defaultSize;

    private static (decimal? Value, string? Invalid) ReadPrice(IQueryCollection source, string key)
    {
        string? raw = source[key];

        if (string.IsNullOrWhiteSpace(raw))
            return (null, null);

        return decimal.TryParse(raw, PriceStyles, CultureInfo.InvariantCulture, out decimal parsed)
            ? (parsed, null)
            : (null, raw);
    }
}