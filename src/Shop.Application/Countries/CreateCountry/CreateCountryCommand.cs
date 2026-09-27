namespace Shop.Application.Countries.CreateCountry;

public sealed record CreateCountryCommand(string? Name, string? Slug);