namespace Shop.Domain.ProductVariants;

public static class UnitOfMeasureExtensions
{
    public static string ToSymbol(this UnitOfMeasure unit) => unit switch
    {
        UnitOfMeasure.Milliliter => "мл",
        UnitOfMeasure.Liter => "л",
        UnitOfMeasure.Gram => "г",
        UnitOfMeasure.Kilogram => "кг",
        UnitOfMeasure.Piece => "шт",
        _ => throw new ArgumentOutOfRangeException(nameof(unit), $"Unknown unit: {unit}")
    };
}