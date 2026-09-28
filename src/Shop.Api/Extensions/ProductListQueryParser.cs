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

    public static Result<ProductFilterSet, Error> ParseFilters(IQueryCollection source)
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

            string[] valueKeys = [.. pair.Value
                .SelectMany(raw => (raw ?? string.Empty).Split(
                    ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Distinct(StringComparer.Ordinal)];

            if (valueKeys.Length == 0)
                continue;

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
        Result<ProductFilterSet, Error> filters = ParseFilters(source);

        if (filters.IsFailure)
            return filters.Error;

        int page = int.TryParse(source["page"], NumberStyles.Integer, CultureInfo.InvariantCulture,
            out int parsedPage) && parsedPage > 0 ? parsedPage : 1;

        int pageSize = int.TryParse(source["pageSize"], NumberStyles.Integer,
            CultureInfo.InvariantCulture, out int parsedSize)
            ? Math.Clamp(parsedSize, 1, ProductListQuery.MaxPageSize)
            : ProductListQuery.DefaultPageSize;

        return new ProductListQuery(categoryId, filters.Value, source["sort"], page, pageSize);
    }

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