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
    public const int MaxMetaTitleLength = 200;
    public const int MaxMetaDescriptionLength = 500;

    private Category(Guid id, string name, Slug slug, Guid? parentId, int displayOrder) : base(id)
    {
        Name = name;
        Slug = slug;
        ParentId = parentId;
        DisplayOrder = displayOrder;
    }

    private Category() { }

    public string Name { get; private set; } = null!;
    public Slug Slug { get; private set; } = null!;
    public Guid? ParentId { get; private set; }
    public int DisplayOrder { get; private set; }
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public IReadOnlyList<CategoryAttribute> Attributes => _attributes;

    public static Result<Category, Error> Create(string? name, Slug slug, Guid? parentId, int displayOrder)
    {
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentOutOfRangeException.ThrowIfNegative(displayOrder);

        if (parentId == Guid.Empty)
            throw new ArgumentException("Parent id must not be empty.", nameof(parentId));

        Result<string, Error> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return normalizedName.Error;

        return new Category(Guid.CreateVersion7(), normalizedName.Value, slug, parentId, displayOrder);
    }

    public UnitResult<Error> Rename(string? name)
    {
        Result<string, Error> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return normalizedName.Error;

        Name = normalizedName.Value;
        return default;
    }

    public void ChangeSlug(Slug slug)
    {
        ArgumentNullException.ThrowIfNull(slug);

        Slug = slug;
    }

    /// <summary>Позицию назначает хендлер: порядок — правило уровня, а не одной категории.</summary>
    public void SetDisplayOrder(int displayOrder)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(displayOrder);

        DisplayOrder = displayOrder;
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


    /// <summary>
    /// Заголовок и описание для поисковика. Пустая строка — это «стереть», поэтому она
    /// превращается в null: отличать «не заполнено» от «заполнено пустым» здесь незачем.
    /// Ограничения длины — техническая граница, а не рекомендация по SEO: подсказать
    /// про 60 и 160 символов должна админка, обрезать за человека мы не вправе.
    /// </summary>
    public UnitResult<Error> SetSeo(string? metaTitle, string? metaDescription)
    {
        string? title = NormalizeMeta(metaTitle);
        string? description = NormalizeMeta(metaDescription);

        if (title is not null && title.Length > MaxMetaTitleLength)
            return DomainErrors.Categories.MetaTitleTooLong(MaxMetaTitleLength);

        if (description is not null && description.Length > MaxMetaDescriptionLength)
            return DomainErrors.Categories.MetaDescriptionTooLong(MaxMetaDescriptionLength);

        MetaTitle = title;
        MetaDescription = description;

        return default;
    }

    private static string? NormalizeMeta(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.CollapseWhitespace();

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