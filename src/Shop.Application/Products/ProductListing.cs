using CSharpFunctionalExtensions;
using Shop.Domain.Errors;

namespace Shop.Application.Products;

public sealed record ProductFilter(string GroupKey, IReadOnlyList<string> ValueKeys)
{
    public const string AttributePrefix = "a.";
    public const string ManufacturerKey = "manufacturer";
    public const string CountryKey = "country";
    public const string PackagingKey = "packaging";
    public const string CategoryKey = "category";
}

public sealed record ProductFilterSet(
    IReadOnlyList<ProductFilter> Filters,
    decimal? PriceMin,
    decimal? PriceMax,
    string? Search = null)
{
    public const string PriceMinKey = "priceMin";
    public const string PriceMaxKey = "priceMax";

    /// <summary>Не ключ адреса, а опознание цены как группы — нужно, чтобы границы
    /// диапазона считались без учёта самого фильтра по цене.</summary>
    public const string PriceGroupKey = "price";

    public const string SearchKey = "q";
    public const int MinSearchLength = 2;
    public const int MaxSearchLength = 100;
}

public sealed record ProductListQuery(
    Guid? CategoryId,
    ProductFilterSet Filters,
    string? Sort,
    int Page,
    int PageSize)
{
    public const int DefaultPageSize = 24;
    public const int MaxPageSize = 60;
}

public sealed record ProductImageThumbnail(string ThumbUrl, string CardUrl, string? Alt);

public sealed record ProductListItemResponse(
    Guid VariantId,
    Guid ProductId,
    string Name,
    string Slug,
    string Sku,
    decimal Price,
    int StockQuantity,
    string ManufacturerName,
    ProductImageThumbnail? Image);

public sealed record ProductListResponse(
    IReadOnlyList<ProductListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public static class FeaturedProducts
{
    public const int DefaultLimit = 12;
    public const int MaxLimit = 24;
}

public interface IProductListQueries
{
    Task<Result<ProductListResponse, Error>> ListAsync(
        ProductListQuery query, CancellationToken cancellationToken);

    /// <summary>Подборка главной: по одной карточке на отмеченный товар.</summary>
    Task<IReadOnlyList<ProductListItemResponse>> GetFeaturedAsync(
        int limit, CancellationToken cancellationToken);
}