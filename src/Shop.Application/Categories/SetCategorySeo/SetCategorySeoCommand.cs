namespace Shop.Application.Categories.SetCategorySeo;

public sealed record SetCategorySeoCommand(
    Guid CategoryId,
    string? MetaTitle,
    string? MetaDescription);
