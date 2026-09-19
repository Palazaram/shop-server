namespace Shop.Application.Categories.SetCategoryAttributes;

public sealed record SetCategoryAttributesCommand(Guid CategoryId, IReadOnlyList<Guid>? AttributeIds);