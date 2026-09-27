namespace Shop.Application.Categories.ChangeCategorySlug;

public sealed record ChangeCategorySlugCommand(Guid CategoryId, string? Slug);