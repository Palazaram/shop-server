namespace Shop.Application.Products.ReorderProductImages;

public sealed record ReorderProductImagesRequest(IReadOnlyList<Guid>? ImageIds);