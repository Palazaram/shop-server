namespace Shop.Application.Manufacturers.UpdateManufacturer;

public sealed record UpdateManufacturerRequest(string? Name, Guid? CountryId);