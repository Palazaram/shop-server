using Shop.Domain.ProductVariants;

namespace Shop.Application.ProductVariants.CreateProductVariant;

public sealed record CreateProductVariantRequest(
    string? Sku,
    decimal? PackagingValue,
    UnitOfMeasure? PackagingUnit,
    decimal? Price,
    int? StockQuantity,
    string? Slug);