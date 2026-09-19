namespace Shop.Application.Products.SetProductAttributeValues;

public sealed record SetProductAttributeValuesRequest(IReadOnlyList<Guid>? ValueIds);