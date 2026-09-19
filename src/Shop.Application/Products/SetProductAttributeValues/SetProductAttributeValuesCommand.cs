namespace Shop.Application.Products.SetProductAttributeValues;

public sealed record SetProductAttributeValuesCommand(Guid ProductId, IReadOnlyList<Guid>? ValueIds);