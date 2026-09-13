namespace Shop.Application.Products.CreateProduct;

public sealed record CreateProductCommand(
    string? Name,
    string? Description,
    Guid? CategoryId,
    Guid? ManufacturerId);