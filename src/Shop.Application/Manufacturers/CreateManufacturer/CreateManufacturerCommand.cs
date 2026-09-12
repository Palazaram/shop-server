namespace Shop.Application.Manufacturers.CreateManufacturer;

public sealed record CreateManufacturerCommand(string? Name, string? Country, string? Slug);