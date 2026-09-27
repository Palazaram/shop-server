namespace Shop.Application.ProductVariants.ChangeProductVariantSlug;

public sealed record ChangeProductVariantSlugCommand(Guid VariantId, string? Slug);