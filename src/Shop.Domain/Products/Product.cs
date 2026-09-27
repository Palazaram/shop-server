using CSharpFunctionalExtensions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Errors;

namespace Shop.Domain.Products;

public sealed class Product : AggregateRoot<Guid>
{
    private readonly List<ProductAttributeValue> _attributeValues = [];
    private readonly List<ProductImage> _images = [];

    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 4000;
    public const int MaxImages = 10;

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
    public IReadOnlyList<ProductAttributeValue> AttributeValues => _attributeValues;
    public IReadOnlyList<ProductImage> Images => _images;

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