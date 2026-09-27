namespace Shop.Application.Countries.ChangeCountrySlug;

public sealed record ChangeCountrySlugCommand(Guid CountryId, string? Slug);