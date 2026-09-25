namespace Shop.Application.Categories.ReorderCategories;

public sealed record ReorderCategoriesCommand(Guid? ParentId, IReadOnlyList<Guid>? CategoryIds);