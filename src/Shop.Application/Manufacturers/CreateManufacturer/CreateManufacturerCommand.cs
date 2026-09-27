namespace Shop.Application.Manufacturers.CreateManufacturer;

public sealed record CreateManufacturerCommand(string? Name, Guid? CountryId, string? Slug);