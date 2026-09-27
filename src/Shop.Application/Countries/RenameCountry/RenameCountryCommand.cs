namespace Shop.Application.Countries.RenameCountry;

public sealed record RenameCountryCommand(Guid CountryId, string? Name);