using CSharpFunctionalExtensions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Errors;

namespace Shop.Domain.Products;

public sealed class Product : AggregateRoot<Guid>
{
    private readonly List<ProductAttributeValue> _attributeValues = [];
    private readonly List<ProductImage> _images = [];
    private readonly List<ProductSpecification> _specifications = [];

    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 4000;
    public const int MaxImages = 10;
    public const int MaxMetaTitleLength = 200;
    public const int MaxMetaDescriptionLength = 500;

    private Product(
        Guid id,
        string name,
        string? description,
        Guid categoryId,
        Guid manufacturerId) : base(id)
    {
        Name = name;
        Description = description;
        CategoryId = categoryId;
        ManufacturerId = manufacturerId;
    }

    private Product() { }

    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid CategoryId { get; private set; }
    public Guid ManufacturerId { get; private set; }
    public bool IsFeatured { get; private set; }
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }
    public IReadOnlyList<ProductAttributeValue> AttributeValues => _attributeValues;
    public IReadOnlyList<ProductImage> Images => _images;
    public IReadOnlyList<ProductSpecification> Specifications => _specifications;

    public static Result<Product, Error> Create(
        string? name,
        string? description,
        Guid categoryId,
        Guid manufacturerId)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category id must not be empty.", nameof(categoryId));

        if (manufacturerId == Guid.Empty)
            throw new ArgumentException("Manufacturer id must not be empty.", nameof(manufacturerId));

        Result<string, Error> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return normalizedName.Error;

        Result<string?, Error> normalizedDescription = NormalizeDescription(description);
        if (normalizedDescription.IsFailure)
            return normalizedDescription.Error;

        return new Product(
            Guid.CreateVersion7(),
            normalizedName.Value,
            normalizedDescription.Value,
            categoryId,
            manufacturerId);
    }

    public UnitResult<Error> Rename(string? name)
    {
        Result<string, Error> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return normalizedName.Error;

        Name = normalizedName.Value;
        return default;
    }

    public UnitResult<Error> ChangeDescription(string? description)
    {
        Result<string?, Error> normalizedDescription = NormalizeDescription(description);
        if (normalizedDescription.IsFailure)
            return normalizedDescription.Error;

        Description = normalizedDescription.Value;
        return default;
    }

    public void MoveToCategory(Guid categoryId)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException("Category id must not be empty.", nameof(categoryId));

        CategoryId = categoryId;
    }

    public void ChangeManufacturer(Guid manufacturerId)
    {
        if (manufacturerId == Guid.Empty)
            throw new ArgumentException("Manufacturer id must not be empty.", nameof(manufacturerId));

        ManufacturerId = manufacturerId;
    }

    /// <summary>
    /// Подборка главной страницы. Флаг стоит на препарате, а не на фасовке: на главную
    /// выносят товар, а какую из его фасовок показать — решает выдача.
    /// </summary>
    public void SetFeatured(bool isFeatured) => IsFeatured = isFeatured;

    public UnitResult<Error> SetAttributeValues(IReadOnlyList<Guid> valueIds)
    {
        ArgumentNullException.ThrowIfNull(valueIds);

        if (valueIds.Any(id => id == Guid.Empty))
            throw new ArgumentException("Attribute value id must not be empty.", nameof(valueIds));

        if (valueIds.Distinct().Count() != valueIds.Count)
            return DomainErrors.Products.DuplicateAttributeValue();

        _attributeValues.Clear();

        foreach (Guid valueId in valueIds)
            _attributeValues.Add(new ProductAttributeValue(Id, valueId));

        return default;
    }

    /// <summary>
    /// Набор задаётся целиком: пустой список снимает все характеристики. Порядок показа
    /// хранится в справочнике, а не здесь, — иначе его пришлось бы держать одинаковым
    /// у каждого товара.
    /// </summary>
    public UnitResult<Error> SetSpecifications(IReadOnlyList<SpecificationValue> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        if (values.Any(value => value.SpecificationId == Guid.Empty))
            throw new ArgumentException(
                "Specification id must not be empty.", nameof(values));

        if (values.Select(value => value.SpecificationId).Distinct().Count() != values.Count)
            return DomainErrors.Products.DuplicateSpecification();

        List<ProductSpecification> replacement = [];

        foreach (SpecificationValue value in values)
        {
            Result<string, Error> normalized = NormalizeSpecificationValue(value.Value);
            if (normalized.IsFailure)
                return normalized.Error;

            replacement.Add(new ProductSpecification(
                Id, value.SpecificationId, normalized.Value));
        }

        _specifications.Clear();
        _specifications.AddRange(replacement);

        return default;
    }

    /// <summary>
    /// Новое изображение встаёт последним. Файлы кладёт вызывающий: домен знает только порядок
    /// и подпись, а путь на диске выводится из Id товара и Id изображения.
    /// </summary>
    public Result<ProductImage, Error> AddImage(string? alt)
    {
        if (_images.Count >= MaxImages)
            return DomainErrors.Products.TooManyImages(MaxImages);

        Result<string?, Error> normalizedAlt = NormalizeAlt(alt);
        if (normalizedAlt.IsFailure)
            return normalizedAlt.Error;

        var image = new ProductImage(Guid.CreateVersion7(), Id, _images.Count, normalizedAlt.Value);

        _images.Add(image);

        return image;
    }

    public UnitResult<Error> RemoveImage(Guid imageId)
    {
        ProductImage? image = _images.SingleOrDefault(candidate => candidate.Id == imageId);

        if (image is null)
            return DomainErrors.Products.ImageNotFound();

        _images.Remove(image);
        RenumberImages();

        return default;
    }

    /// <summary>
    /// Список обязан содержать все изображения товара целиком — как и у категорий:
    /// частичная перестановка оставила бы порядок неопределённым.
    /// </summary>
    public UnitResult<Error> ReorderImages(IReadOnlyList<Guid> imageIds)
    {
        ArgumentNullException.ThrowIfNull(imageIds);

        if (imageIds.Distinct().Count() != imageIds.Count)
            return DomainErrors.Products.DuplicateImageInOrder();

        if (imageIds.Count != _images.Count)
            return DomainErrors.Products.ImageOrderIsIncomplete(_images.Count);

        Dictionary<Guid, ProductImage> byId = _images.ToDictionary(image => image.Id);

        foreach (Guid imageId in imageIds)
            if (!byId.ContainsKey(imageId))
                return DomainErrors.Products.ImageNotOnProduct(imageId);

        for (int index = 0; index < imageIds.Count; index++)
            byId[imageIds[index]].SetDisplayOrder(index);

        return default;
    }

    public UnitResult<Error> ChangeImageAlt(Guid imageId, string? alt)
    {
        ProductImage? image = _images.SingleOrDefault(candidate => candidate.Id == imageId);

        if (image is null)
            return DomainErrors.Products.ImageNotFound();

        Result<string?, Error> normalizedAlt = NormalizeAlt(alt);
        if (normalizedAlt.IsFailure)
            return normalizedAlt.Error;

        image.SetAlt(normalizedAlt.Value);

        return default;
    }

    private void RenumberImages()
    {
        int order = 0;

        foreach (ProductImage image in _images.OrderBy(image => image.DisplayOrder))
            image.SetDisplayOrder(order++);
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
            return DomainErrors.Products.MetaTitleTooLong(MaxMetaTitleLength);

        if (description is not null && description.Length > MaxMetaDescriptionLength)
            return DomainErrors.Products.MetaDescriptionTooLong(MaxMetaDescriptionLength);

        MetaTitle = title;
        MetaDescription = description;

        return default;
    }

    private static string? NormalizeMeta(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.CollapseWhitespace();

    private static Result<string, Error> NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DomainErrors.Products.NameIsRequired();

        string normalized = name.CollapseWhitespace();

        if (normalized.Length > MaxNameLength)
            return DomainErrors.Products.NameTooLong(MaxNameLength);

        return normalized;
    }

    private static Result<string?, Error> NormalizeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
            return Result.Success<string?, Error>(null);

        string normalized = description.Trim();

        if (normalized.Length > MaxDescriptionLength)
            return DomainErrors.Products.DescriptionTooLong(MaxDescriptionLength);

        return normalized;
    }

    private static Result<string, Error> NormalizeSpecificationValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DomainErrors.Products.SpecificationValueIsRequired();

        string normalized = value.CollapseWhitespace();

        if (normalized.Length > ProductSpecification.MaxValueLength)
            return DomainErrors.Products.SpecificationValueTooLong(
                ProductSpecification.MaxValueLength);

        return normalized;
    }

    private static Result<string?, Error> NormalizeAlt(string? alt)
    {
        if (string.IsNullOrWhiteSpace(alt))
            return Result.Success<string?, Error>(null);

        string normalized = alt.CollapseWhitespace();

        if (normalized.Length > ProductImage.MaxAltLength)
            return DomainErrors.Products.ImageAltTooLong(ProductImage.MaxAltLength);

        return normalized;
    }
}