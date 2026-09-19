namespace Shop.Domain.Categories;

public sealed class CategoryAttribute
{
    private CategoryAttribute()
    {
    }

    internal CategoryAttribute(Guid categoryId, Guid attributeId, int displayOrder)
    {
        CategoryId = categoryId;
        AttributeId = attributeId;
        DisplayOrder = displayOrder;
    }

    public Guid CategoryId { get; private set; }
    public Guid AttributeId { get; private set; }
    public int DisplayOrder { get; private set; }
}