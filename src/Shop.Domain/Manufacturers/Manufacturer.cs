using CSharpFunctionalExtensions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Errors;

namespace Shop.Domain.Manufacturers;

public sealed class Manufacturer : AggregateRoot<Guid>
{
    public const int MaxNameLength = 100;

    private Manufacturer(Guid id, string name, Slug slug, Guid countryId) : base(id)
    {
        Name = name;
        Slug = slug;
        CountryId = countryId;
    }

    private Manufacturer()
    {
    }

    public string Name { get; private set; } = null!;
    public Slug Slug { get; private set; } = null!;
    public Guid CountryId { get; private set; }

    public static Result<Manufacturer, Error> Create(string? name, Slug slug, Guid countryId)
    {
        ArgumentNullException.ThrowIfNull(slug);

        if (countryId == Guid.Empty)
            throw new ArgumentException("Country id must not be empty.", nameof(countryId));

        Result<string, Error> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return normalizedName.Error;

        return new Manufacturer(Guid.CreateVersion7(), normalizedName.Value, slug, countryId);
    }

    public UnitResult<Error> Rename(string? name)
    {
        Result<string, Error> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return normalizedName.Error;

        Name = normalizedName.Value;
        return UnitResult.Success<Error>();
    }

    public void ChangeSlug(Slug slug)
    {
        ArgumentNullException.ThrowIfNull(slug);

        Slug = slug;
    }

    /// <summary>Существование страны проверяет хендлер: это правило про связь двух агрегатов.</summary>
    public void ChangeCountry(Guid countryId)
    {
        if (countryId == Guid.Empty)
            throw new ArgumentException("Country id must not be empty.", nameof(countryId));

        CountryId = countryId;
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
}