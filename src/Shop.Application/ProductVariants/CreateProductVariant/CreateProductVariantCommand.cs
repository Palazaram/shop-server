using Shop.Domain.ProductVariants;

namespace Shop.Application.ProductVariants.CreateProductVariant;

public sealed record CreateProductVariantCommand(
    Guid ProductId,
    string? Sku,
    decimal? PackagingValue,
    UnitOfMeasure? PackagingUnit,
    decimal? Price,
    int? StockQuantity,
    string? Slug);