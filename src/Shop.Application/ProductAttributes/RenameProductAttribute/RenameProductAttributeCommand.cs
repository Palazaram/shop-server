namespace Shop.Application.ProductAttributes.RenameProductAttribute;

public sealed record RenameProductAttributeCommand(Guid AttributeId, string? Name);