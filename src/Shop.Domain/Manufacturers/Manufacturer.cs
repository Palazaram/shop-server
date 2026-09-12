using CSharpFunctionalExtensions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Errors;

namespace Shop.Domain.Manufacturers;

public sealed class Manufacturer : AggregateRoot<Guid>
{
    public const int MaxNameLength = 100;
    public const int MaxCountryLength = 60;

    private Manufacturer(Guid id, string name, Slug slug, string country) : base(id)
    {
        Name = name;
        Slug = slug;
        Country = country;
    }

    private Manufacturer()
    {
    }

    public string Name { get; private set; } = null!;
    public Slug Slug { get; private set; } = null!;
    public string Country { get; private set; } = null!;

    public static Result<Manufacturer, Error> Create(string? name, Slug slug, string? country)
    {
        ArgumentNullException.ThrowIfNull(slug);

        Result<string, Error> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return normalizedName.Error;

        Result<string, Error> normalizedCountry = NormalizeCountry(country);
        if (normalizedCountry.IsFailure)
            return normalizedCountry.Error;

        return new Manufacturer(
            Guid.CreateVersion7(),
            normalizedName.Value,
            slug,
            normalizedCountry.Value);
    }

    public UnitResult<Error> Rename(string? name)
    {
        Result<string, Error> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return normalizedName.Error;

        Name = normalizedName.Value;
        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> ChangeCountry(string? country)
    {
        Result<string, Error> normalizedCountry = NormalizeCountry(country);
        if (normalizedCountry.IsFailure)
            return normalizedCountry.Error;

        Country = normalizedCountry.Value;
        return UnitResult.Success<Error>();
    }

    private static Result<string, Error> NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DomainErrors.Manufacturers.NameIsRequired();

        string normalized = name.CollapseWhitespace();

        if (normalized.Length > MaxNameLength)
            return DomainErrors.Manufacturers.NameTooLong(MaxNameLength);

        return normalized;
    }

    private static Result<string, Error> NormalizeCountry(string? country)
    {
        if (string.IsNullOrWhiteSpace(country))
            return DomainErrors.Manufacturers.CountryIsRequired();

        string normalized = country.CollapseWhitespace();

        if (normalized.Length > MaxCountryLength)
            return DomainErrors.Manufacturers.CountryTooLong(MaxCountryLength);

        return normalized;
    }
}