namespace Shop.Application.Products.ChangeProductImageAlt;

public sealed record ChangeProductImageAltCommand(Guid ProductId, Guid ImageId, string? Alt);