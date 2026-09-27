namespace Shop.Domain.Products;

public sealed class ProductImage
{
    public const int MaxAltLength = 200;

    private ProductImage() { }

    internal ProductImage(Guid id, Guid productId, int displayOrder, string? alt)
    {
        Id = id;
        ProductId = productId;
        DisplayOrder = displayOrder;
        Alt = alt;
    }

    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public int DisplayOrder { get; private set; }
    public string? Alt { get; private set; }

    internal void SetDisplayOrder(int displayOrder) => DisplayOrder = displayOrder;

    internal void SetAlt(string? alt) => Alt = alt;
}