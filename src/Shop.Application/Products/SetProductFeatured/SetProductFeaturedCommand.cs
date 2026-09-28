namespace Shop.Application.Products.SetProductFeatured;

public sealed record SetProductFeaturedCommand(Guid ProductId, bool? IsFeatured);
