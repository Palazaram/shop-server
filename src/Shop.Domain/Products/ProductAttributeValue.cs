namespace Shop.Domain.Products;

public sealed class ProductAttributeValue
{
    private ProductAttributeValue()
    {
    }

    internal ProductAttributeValue(Guid productId, Guid attributeValueId)
    {
        ProductId = productId;
        AttributeValueId = attributeValueId;
    }

    public Guid ProductId { get; private set; }
    public Guid AttributeValueId { get; private set; }
}