namespace Shop.Application.Categories.RenameCategory;

public sealed record RenameCategoryCommand(Guid CategoryId, string? Name);