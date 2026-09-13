namespace Shop.Application.ProductVariants.UpdateProductVariant;

public sealed record UpdateProductVariantRequest(
    string? Sku,
    decimal? Price,
    int? StockQuantity);