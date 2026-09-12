namespace Shop.Application.Manufacturers.UpdateManufacturer;

public sealed record UpdateManufacturerCommand(Guid ManufacturerId, string? Name, string? Country);