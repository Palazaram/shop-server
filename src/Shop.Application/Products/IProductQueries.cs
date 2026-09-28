using CSharpFunctionalExtensions;
using Shop.Application.ProductAttributes;
using Shop.Domain.Errors;

namespace Shop.Application.Products;

public sealed record ProductAttributeValueGroupResponse(
    Guid AttributeId,
    string AttributeName,
    string AttributeSlug,
    IReadOnlyList<AttributeValueResponse> Values);

/// <summary>
/// Админский список показывает <b>препараты</b>, поэтому у него свой узкий набор параметров:
/// фасовка и цена сюда не входят — они про фасовки, и молча проигнорированный фильтр
/// был бы хуже его отсутствия.
/// </summary>
public sealed record AdminProductListQuery(
    string? Search,
    IReadOnlyList<string> CategorySlugs,
    IReadOnlyList<string> ManufacturerSlugs,
    bool? IsFeatured,
    string? Sort,
    int Page,
    int PageSize)
{
    public const string FeaturedKey = "featured";
    public const int DefaultPageSize = 25;
    public const int MaxPageSize = 100;
}

public sealed record AdminProductListItemResponse(
    Guid Id,
    string Name,
    string CategoryName,
    string ManufacturerName,
    int VariantCount,
    int ImageCount,
    bool IsFeatured);

public sealed record AdminProductListResponse(
    IReadOnlyList<AdminProductListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public interface IProductQueries
{
    Task<Maybe<IReadOnlyList<ProductAttributeValueGroupResponse>>> GetAttributeValuesAsync(
        Guid productId,
        CancellationToken cancellationToken);

    Task<Result<AdminProductListResponse, Error>> ListAsync(
        AdminProductListQuery query,
        CancellationToken cancellationToken);
}