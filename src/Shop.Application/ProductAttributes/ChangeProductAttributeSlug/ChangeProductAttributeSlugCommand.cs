namespace Shop.Application.ProductAttributes.ChangeProductAttributeSlug;

public sealed record ChangeProductAttributeSlugCommand(Guid AttributeId, string? Slug);