namespace Shop.Application.Products.UpdateProduct;

public sealed record UpdateProductCommand(
    Guid ProductId,
    string? Name,
    string? Description,
    Guid? CategoryId,
    Guid? ManufacturerId);