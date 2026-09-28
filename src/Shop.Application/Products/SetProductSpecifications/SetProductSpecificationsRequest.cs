namespace Shop.Application.Products.SetProductSpecifications;

public sealed record SetProductSpecificationsRequest(
    IReadOnlyList<ProductSpecificationItem>? Specifications);
