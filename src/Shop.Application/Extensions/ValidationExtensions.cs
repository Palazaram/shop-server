using CSharpFunctionalExtensions;
using FluentValidation;
using FluentValidation.Results;
using Shop.Domain.Errors;

namespace Shop.Application.Extensions;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, TProperty> WithError<T, TProperty>(
        this IRuleBuilderOptions<T, TProperty> rule, Error error)
        => rule
            .WithErrorCode(error.Code)
            .WithMessage(error.Message);

    public static IRuleBuilderOptionsConditions<T, TElement> MustBeValueObject<T, TElement, TValueObject>(
        this IRuleBuilder<T, TElement> ruleBuilder,
        Func<TElement, Result<TValueObject, Error>> factoryMethod)
        where TValueObject : ValueObject
    {
        return ruleBuilder.Custom((value, context) =>
        {
            Result<TValueObject, Error> result = factoryMethod(value);

            if (result.IsFailure)
            {
                context.AddFailure(new ValidationFailure(context.PropertyPath, result.Error.Message)
                {
                    ErrorCode = result.Error.Code
                });
            }
        });
    }

    public static IRuleBuilderOptions<T, string> ValidPassword<T>(this IRuleBuilder<T, string> rule)
    {
        const int minLength = 8;

        return rule
            .NotEmpty().WithError(DomainErrors.Users.PasswordIsRequired())
            .MinimumLength(minLength).WithError(DomainErrors.Users.PasswordTooShort(minLength))
            .Matches("[A-Z]").WithError(DomainErrors.Users.PasswordMissingUppercase())
            .Matches("[0-9]").WithError(DomainErrors.Users.PasswordMissingDigit());
    }
}