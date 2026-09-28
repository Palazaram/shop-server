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

    // Для адресов. В отличие от ToSymbol(), который можно переписать хоть завтра,
    // эта таблица уходит в слаги и остаётся в ссылках навсегда.
    public static string ToSlugSymbol(this UnitOfMeasure unit) => unit switch
    {
        UnitOfMeasure.Milliliter => "ml",
        UnitOfMeasure.Liter => "l",
        UnitOfMeasure.Gram => "g",
        UnitOfMeasure.Kilogram => "kg",
        UnitOfMeasure.Piece => "pcs",
        _ => throw new ArgumentOutOfRangeException(nameof(unit), $"Unknown unit: {unit}")
    };
}