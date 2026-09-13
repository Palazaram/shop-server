namespace Shop.Application.Products.UpdateProduct;

public sealed record UpdateProductRequest(
    string? Name,
    string? Description,
    Guid? CategoryId,
    Guid? ManufacturerId);