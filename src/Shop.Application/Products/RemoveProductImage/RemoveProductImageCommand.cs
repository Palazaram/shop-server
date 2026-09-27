namespace Shop.Application.Products.RemoveProductImage;

public sealed record RemoveProductImageCommand(Guid ProductId, Guid ImageId);