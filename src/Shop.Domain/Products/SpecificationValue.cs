namespace Shop.Domain.Products;

/// <summary>
/// Пара «характеристика — значение» на входе в <see cref="Product.SetSpecifications"/>.
/// </summary>
public sealed record SpecificationValue(Guid SpecificationId, string? Value);
