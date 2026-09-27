using CSharpFunctionalExtensions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Errors;

namespace Shop.Domain.Countries;

public sealed class Country : AggregateRoot<Guid>
{
    public const int MaxNameLength = 60;

    private Country(Guid id, string name, Slug slug) : base(id)
    {
        Name = name;
        Slug = slug;
    }

    private Country() { }

    public string Name { get; private set; } = null!;
    public Slug Slug { get; private set; } = null!;

    public static Result<Country, Error> Create(string? name, Slug slug)
    {
        ArgumentNullException.ThrowIfNull(slug);

        Result<string, Error> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return normalizedName.Error;

        return new Country(Guid.CreateVersion7(), normalizedName.Value, slug);
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

    private static Result<string, Error> NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DomainErrors.Countries.NameIsRequired();

        string normalized = name.CollapseWhitespace();

        if (normalized.Length > MaxNameLength)
            return DomainErrors.Countries.NameTooLong(MaxNameLength);

        return normalized;
    }
}