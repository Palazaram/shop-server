namespace Shop.Application.Products.SetProductSpecifications;

public sealed record SetProductSpecificationsCommand(
    Guid ProductId,
    IReadOnlyList<ProductSpecificationItem>? Specifications);

public sealed record ProductSpecificationItem(Guid? SpecificationId, string? Value);
