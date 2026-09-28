using CSharpFunctionalExtensions;

namespace Shop.Application.ProductVariants;

public sealed record ProductImageResponse(
    string ThumbUrl,
    string CardUrl,
    string FullUrl,
    string? Alt);

public sealed record ProductVariantListItemResponse(
    Guid Id,
    string Sku,
    string Slug,
    string Packaging,
    decimal Price,
    int StockQuantity,
    bool IsInStock);

/// <summary>
/// Строка таблицы характеристик в карточке. Сюда сведены и значения атрибутов
/// («Діюча речовина»), и характеристики из справочника («Клас токсичності»): покупателю
/// всё равно, что из этого участвует в фильтрах.
/// </summary>
public sealed record ProductSpecificationResponse(string Name, string Value);

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
    string? MetaTitle,
    string? MetaDescription,
    string CanonicalSlug,
    IReadOnlyList<ProductImageResponse> Images,
    IReadOnlyList<ProductSpecificationResponse> Specifications,
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