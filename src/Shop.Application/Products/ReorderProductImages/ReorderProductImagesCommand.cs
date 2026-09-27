namespace Shop.Application.Products.ReorderProductImages;

public sealed record ReorderProductImagesCommand(Guid ProductId, IReadOnlyList<Guid>? ImageIds);