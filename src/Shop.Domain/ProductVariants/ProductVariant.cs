using CSharpFunctionalExtensions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Errors;

namespace Shop.Domain.ProductVariants;

public sealed class ProductVariant : AggregateRoot<Guid>
{
    public const int MaxSkuLength = 50;

    private ProductVariant(
        Guid id,
        Guid productId,
        string sku,
        Slug slug,
        Packaging packaging,
        Money price,
        int stockQuantity) : base(id)
    {
        ProductId = productId;
        Sku = sku;
        Slug = slug;
        Packaging = packaging;
        Price = price;
        StockQuantity = stockQuantity;
    }

    private ProductVariant() { }

    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = null!;
    public Slug Slug { get; private set; } = null!;
    public Packaging Packaging { get; private set; } = null!;
    public Money Price { get; private set; } = null!;
    public int StockQuantity { get; private set; }

    public bool IsInStock => StockQuantity > 0;

    public static Result<ProductVariant, Error> Create(
        Guid productId,
        string? sku,
        Slug slug,
        Packaging packaging,
        Money price,
        int stockQuantity)
    {
        ArgumentNullException.ThrowIfNull(slug);
        ArgumentNullException.ThrowIfNull(packaging);
        ArgumentNullException.ThrowIfNull(price);

        if (productId == Guid.Empty)
            throw new ArgumentException("Product id must not be empty.", nameof(productId));

        Result<string, Error> normalizedSku = NormalizeSku(sku);
        if (normalizedSku.IsFailure)
            return normalizedSku.Error;

        if (stockQuantity < 0)
            return DomainErrors.ProductVariants.StockMustNotBeNegative();

        return new ProductVariant(
            Guid.CreateVersion7(),
            productId,
            normalizedSku.Value,
            slug,
            packaging,
            price,
            stockQuantity);
    }

    public UnitResult<Error> ChangeSku(string? sku)
    {
        Result<string, Error> normalizedSku = NormalizeSku(sku);
        if (normalizedSku.IsFailure)
            return normalizedSku.Error;

        Sku = normalizedSku.Value;
        return default;
    }

    public void ChangePrice(Money price)
    {
        ArgumentNullException.ThrowIfNull(price);

        Price = price;
    }

    public UnitResult<Error> SetStock(int quantity)
    {
        if (quantity < 0)
            return DomainErrors.ProductVariants.StockMustNotBeNegative();

        StockQuantity = quantity;
        return default;
    }

    private static Result<string, Error> NormalizeSku(string? sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return DomainErrors.ProductVariants.SkuIsRequired();

        string normalized = sku.Trim();

        if (normalized.Length > MaxSkuLength)
            return DomainErrors.ProductVariants.SkuTooLong(MaxSkuLength);

        return normalized;
    }
}