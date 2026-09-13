namespace Shop.Application.ProductVariants.UpdateProductVariant;

public sealed record UpdateProductVariantCommand(
    Guid VariantId,
    string? Sku,
    decimal? Price,
    int? StockQuantity);