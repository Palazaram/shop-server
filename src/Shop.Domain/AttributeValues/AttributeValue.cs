using CSharpFunctionalExtensions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Errors;

namespace Shop.Domain.AttributeValues;

public sealed class AttributeValue : AggregateRoot<Guid>
{
    public const int MaxNameLength = 100;

    private AttributeValue(Guid id, Guid attributeId, string name, Slug slug) : base(id)
    {
        AttributeId = attributeId;
        Name = name;
        Slug = slug;
    }

    private AttributeValue()
    {
    }

    public Guid AttributeId { get; private set; }
    public string Name { get; private set; } = null!;
    public Slug Slug { get; private set; } = null!;

    public static Result<AttributeValue, Error> Create(Guid attributeId, string? name, Slug slug)
    {
        ArgumentNullException.ThrowIfNull(slug);

        if (attributeId == Guid.Empty)
            throw new ArgumentException("Attribute id must not be empty.", nameof(attributeId));

        Result<string, Error> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return normalizedName.Error;

        return new AttributeValue(
            Guid.CreateVersion7(), attributeId, normalizedName.Value, slug);
    }

    public UnitResult<Error> Rename(string? name)
    {
        Result<string, Error> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return normalizedName.Error;

        Name = normalizedName.Value;
        return UnitResult.Success<Error>();
    }

    private static Result<string, Error> NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DomainErrors.AttributeValues.NameIsRequired();

        string normalized = name.CollapseWhitespace();

        if (normalized.Length > MaxNameLength)
            return DomainErrors.AttributeValues.NameTooLong(MaxNameLength);

        return normalized;
    }
}