namespace Shop.Application.Products.SetProductSeo;

public sealed record SetProductSeoCommand(
    Guid ProductId,
    string? MetaTitle,
    string? MetaDescription);
