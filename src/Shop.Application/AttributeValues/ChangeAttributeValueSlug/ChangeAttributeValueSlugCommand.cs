namespace Shop.Application.AttributeValues.ChangeAttributeValueSlug;

public sealed record ChangeAttributeValueSlugCommand(Guid ValueId, string? Slug);