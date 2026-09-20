using System.Globalization;
using CSharpFunctionalExtensions;
using Shop.Domain.Errors;

namespace Shop.Domain.ProductVariants;

public sealed class Packaging : ValueObject
{
    private Packaging(decimal value, UnitOfMeasure unit)
    {
        Value = value;
        Unit = unit;
        Key = BuildKey(value, unit);
    }

    public decimal Value { get; }
    public UnitOfMeasure Unit { get; }
    public string Key { get; private set; } = null!;

    public static Result<Packaging, Error> Create(decimal value, UnitOfMeasure unit)
    {
        if (!Enum.IsDefined(unit))
            return DomainErrors.Packagings.UnitIsInvalid();

        decimal rounded = decimal.Round(value, 3, MidpointRounding.AwayFromZero);

        if (rounded <= 0)
            return DomainErrors.Packagings.ValueMustBePositive();

        return new Packaging(rounded, unit);
    }

    public override string ToString()
        => $"{Value.ToString("0.###", CultureInfo.InvariantCulture)} {Unit.ToSymbol()}";

    protected override IEnumerable<IComparable> GetEqualityComponents()
    {
        yield return Value;
        yield return Unit;
    }

    // Опознание, в отличие от ToString(), который про отображение.
    // Единица берётся именем члена перечисления, а не символом: символ — это то,
    // что видит человек, и его можно поменять, не ломая ссылки.
    private static string BuildKey(decimal value, UnitOfMeasure unit)
        => $"{value.ToString("0.###", CultureInfo.InvariantCulture)}-{unit.ToString().ToLowerInvariant()}";
}