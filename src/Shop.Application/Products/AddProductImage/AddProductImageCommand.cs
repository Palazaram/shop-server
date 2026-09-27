namespace Shop.Application.Products.AddProductImage;

public sealed record AddProductImageCommand(Guid ProductId, byte[] Content, string? Alt);