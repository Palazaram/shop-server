namespace Shop.Application.Manufacturers.ChangeManufacturerSlug;

public sealed record ChangeManufacturerSlugCommand(Guid ManufacturerId, string? Slug);