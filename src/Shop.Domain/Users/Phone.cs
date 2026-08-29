using CSharpFunctionalExtensions;
using Shop.Domain.Errors;
using System.Text.RegularExpressions;

namespace Shop.Domain.Users;

public sealed partial class Phone : SimpleValueObject<string>
{
    public const int NationalNumberLength = 9;   // 99 628 66 44
    private const string CountryCode = "380";

    [GeneratedRegex(@"^[0-9\s+()-]+$")]
    private static partial Regex AllowedCharacters();

    [GeneratedRegex(@"[^0-9]")]
    private static partial Regex NonDigits();

    private Phone(string value) : base(value) { }

    public static Result<Phone, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DomainErrors.Users.PhoneIsRequired();

        string trimmed = value.Trim();

        // 1. Сначала проверяем, что ввод состоит ТОЛЬКО из разрешённых символов
        if (!AllowedCharacters().IsMatch(trimmed))
            return DomainErrors.Users.PhoneInvalidFormat();

        // 2. Теперь безопасно вычищаем всё, кроме цифр
        string digits = NonDigits().Replace(trimmed, string.Empty);

        // 3. Приводим к национальному формату: +380XX… / 380XX… / 0XX… → XX…
        string national = digits;

        if (national.StartsWith(CountryCode, StringComparison.Ordinal))
            national = national[CountryCode.Length..];
        else if (national.StartsWith('0'))
            national = national[1..];

        if (national.Length != NationalNumberLength)
            return DomainErrors.Users.PhoneInvalidLength(NationalNumberLength);

        return new Phone(national);
    }
}