namespace Shop.Application.Categories.SetCategoryAttributes;

public sealed record SetCategoryAttributesRequest(IReadOnlyList<Guid>? AttributeIds);