using CSharpFunctionalExtensions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Errors;

namespace Shop.Domain.ProductAttributes;

public sealed class ProductAttribute : AggregateRoot<Guid>
{
    public const int MaxNameLength = 100;

    private ProductAttribute(Guid id, string name, Slug slug) : base(id)
    {
        Name = name;
        Slug = slug;
    }

    private ProductAttribute()
    {
    }

    public string Name { get; private set; } = null!;
    public Slug Slug { get; private set; } = null!;

    public static Result<ProductAttribute, Error> Create(string? name, Slug slug)
    {
        ArgumentNullException.ThrowIfNull(slug);

        Result<string, Error> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return normalizedName.Error;

        return new ProductAttribute(Guid.CreateVersion7(), normalizedName.Value, slug);
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
            return DomainErrors.ProductAttributes.NameIsRequired();

        string normalized = name.CollapseWhitespace();

        if (normalized.Length > MaxNameLength)
            return DomainErrors.ProductAttributes.NameTooLong(MaxNameLength);

        return normalized;
    }
}