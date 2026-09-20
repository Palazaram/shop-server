using CSharpFunctionalExtensions;
using Shop.Domain.Errors;

namespace Shop.Application.Products;

public sealed record ProductFilter(string GroupKey, IReadOnlyList<string> ValueKeys)
{
    public const string AttributePrefix = "a.";
    public const string ManufacturerKey = "manufacturer";
    public const string CountryKey = "country";
    public const string PackagingKey = "packaging";
}

public sealed record ProductFilterSet(
    IReadOnlyList<ProductFilter> Filters,
    decimal? PriceMin,
    decimal? PriceMax)
{
    public const string PriceMinKey = "priceMin";
    public const string PriceMaxKey = "priceMax";

    /// <summary>Не ключ адреса, а опознание цены как группы — нужно, чтобы границы
    /// диапазона считались без учёта самого фильтра по цене.</summary>
    public const string PriceGroupKey = "price";
}

public sealed record ProductListQuery(
    Guid CategoryId,
    ProductFilterSet Filters,
    string? Sort,
    int Page,
    int PageSize)
{
    public const int DefaultPageSize = 24;
    public const int MaxPageSize = 60;
}

public sealed record ProductListItemResponse(
    Guid VariantId,
    Guid ProductId,
    string Name,
    string Slug,
    string Sku,
    decimal Price,
    int StockQuantity,
    string ManufacturerName);

public sealed record ProductListResponse(
    IReadOnlyList<ProductListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public interface IProductListQueries
{
    Task<Result<ProductListResponse, Error>> ListAsync(
        ProductListQuery query, CancellationToken cancellationToken);
}