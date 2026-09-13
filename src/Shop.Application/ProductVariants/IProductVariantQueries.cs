using CSharpFunctionalExtensions;

namespace Shop.Application.ProductVariants;

public sealed record ProductVariantListItemResponse(
    Guid Id,
    string Sku,
    string Slug,
    string Packaging,
    decimal Price,
    int StockQuantity,
    bool IsInStock);

public sealed record ProductVariantDetailResponse(
    Guid Id,
    string Slug,
    string Sku,
    string DisplayName,
    string ProductName,
    string? Description,
    string ManufacturerName,
    string ManufacturerCountry,
    string CategoryName,
    string Packaging,
    decimal Price,
    int StockQuantity,
    bool IsInStock,
    IReadOnlyList<ProductVariantListItemResponse> OtherPackagings);

public interface IProductVariantQueries
{
    Task<Maybe<ProductVariantDetailResponse>> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductVariantListItemResponse>> GetByProductAsync(
        Guid productId,
        CancellationToken cancellationToken);
}