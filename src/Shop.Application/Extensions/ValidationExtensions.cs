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

    /// <summary>
    /// NotEmpty() на Guid? сравнивает значение с default(Guid?), то есть с null,
    /// и пропускает Guid.Empty. Это правило проверяет оба случая.
    /// </summary>
    public static IRuleBuilderOptions<T, Guid?> NotEmptyId<T>(this IRuleBuilder<T, Guid?> rule)
        => rule.Must(id => id.HasValue && id.Value != Guid.Empty);

    /// <summary>
    /// Длина и алфавит, больше ничего. Требования к составу («обязательна заглавная»,
    /// «обязательна цифра») сняты намеренно: они заставляют писать Lopata1 вместо длинной
    /// понятной фразы, то есть дают предсказуемый шаблон вместо стойкости.
    /// <para>
    /// Верхняя граница — не придирка: bcrypt использует не более 72 байт пароля, остальное
    /// в хеш не попадает. Без ограничения человек с длинной фразой мог бы ошибиться в её конце
    /// и всё равно войти.
    /// </para>
    /// <para>
    /// Алфавит — печатаемый ASCII без пробела: латиница, цифры, знаки препинания. Кириллица
    /// запрещена намеренно: такой пароль не ввести там, где нет раскладки — на чужом
    /// компьютере или с иностранной клавиатуры телефона. Пробел запрещён заодно: невидимый
    /// пробел, прилипший при копировании, даёт «ввожу правильно, но не пускает».
    /// </para>
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidPassword<T>(this IRuleBuilder<T, string?> rule)
    {
        const int minLength = 8;
        const int maxLength = 64;
        const string printableAsciiNoSpace = "^[!-~]+$";

        return rule
            .NotEmpty().WithError(DomainErrors.Users.PasswordIsRequired())
            .MinimumLength(minLength).WithError(DomainErrors.Users.PasswordTooShort(minLength))
            .MaximumLength(maxLength).WithError(DomainErrors.Users.PasswordTooLong(maxLength))
            .Matches(printableAsciiNoSpace)
                .WithError(DomainErrors.Users.PasswordHasUnsupportedCharacters());
    }
}