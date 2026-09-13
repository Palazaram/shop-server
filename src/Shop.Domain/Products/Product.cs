using CSharpFunctionalExtensions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Errors;

namespace Shop.Domain.Products;

public sealed class Product : AggregateRoot<Guid>
{
    public const int MaxNameLength = 200;
    public const int MaxDescriptionLength = 4000;

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
}