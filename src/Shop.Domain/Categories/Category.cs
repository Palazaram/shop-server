using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Errors;

namespace Shop.Domain.Categories;

public sealed class Category : AggregateRoot<Guid>
{
    private readonly List<CategoryAttribute> _attributes = [];
    public const int MaxNameLength = 100;

    private Category(Guid id, string name, Slug slug, Guid? parentId) : base(id)
    {
        Name = name;
        Slug = slug;
        ParentId = parentId;
    }

    private Category() { }

    public string Name { get; private set; } = null!;
    public Slug Slug { get; private set; } = null!;
    public Guid? ParentId { get; private set; }
    public IReadOnlyList<CategoryAttribute> Attributes => _attributes;

    public static Result<Category, Error> Create(string? name, Slug slug, Guid? parentId)
    {
        ArgumentNullException.ThrowIfNull(slug);

        if (parentId == Guid.Empty)
            throw new ArgumentException("Parent id must not be empty.", nameof(parentId));

        Result<string, Error> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return normalizedName.Error;

        return new Category(Guid.CreateVersion7(), normalizedName.Value, slug, parentId);
    }

    public UnitResult<Error> Rename(string? name)
    {
        Result<string, Error> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return normalizedName.Error;

        Name = normalizedName.Value;
        return default;
    }

    public UnitResult<Error> SetAttributes(IReadOnlyList<Guid> attributeIds)
    {
        ArgumentNullException.ThrowIfNull(attributeIds);

        if (attributeIds.Any(id => id == Guid.Empty))
            throw new ArgumentException("Attribute id must not be empty.", nameof(attributeIds));

        if (attributeIds.Distinct().Count() != attributeIds.Count)
            return DomainErrors.Categories.DuplicateAttribute();

        _attributes.Clear();

        for (int index = 0; index < attributeIds.Count; index++)
            _attributes.Add(new CategoryAttribute(Id, attributeIds[index], index));

        return default;
    }

    private static Result<string, Error> NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DomainErrors.Categories.NameIsRequired();

        string normalized = name.CollapseWhitespace();

        if (normalized.Length > MaxNameLength)
            return DomainErrors.Categories.NameTooLong(MaxNameLength);

        return normalized;
    }
}