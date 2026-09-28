namespace Shop.Domain.Products;

/// <summary>
/// Значение характеристики у товара. Имя приходит из справочника, значение — свободная строка:
/// «II», «20 днів», «1–2 л/га». Справочник значений здесь был бы вреден: пришлось бы заводить
/// запись на каждое число.
/// </summary>
public sealed class ProductSpecification
{
    public const int MaxValueLength = 200;

    private ProductSpecification()
    {
    }

    internal ProductSpecification(Guid productId, Guid specificationId, string value)
    {
        ProductId = productId;
        SpecificationId = specificationId;
        Value = value;
    }

    public Guid ProductId { get; private set; }
    public Guid SpecificationId { get; private set; }
    public string Value { get; private set; } = null!;
}
