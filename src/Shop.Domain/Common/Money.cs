using CSharpFunctionalExtensions;
using Shop.Domain.Errors;

namespace Shop.Domain.Common;

public sealed class Money : SimpleValueObject<decimal>
{
    private Money(decimal value) : base(value) { }

    public static Result<Money, Error> Create(decimal amount)
    {
        decimal rounded = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);

        if (rounded <= 0)
            return DomainErrors.Money.MustBePositive();

        return new Money(rounded);
    }
}