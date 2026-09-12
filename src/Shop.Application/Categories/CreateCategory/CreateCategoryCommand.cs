namespace Shop.Application.Categories.CreateCategory;

public sealed record CreateCategoryCommand(string Name, string? Slug, Guid? ParentId);