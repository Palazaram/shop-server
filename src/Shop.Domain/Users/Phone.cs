using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shop.Domain.Errors;

namespace Shop.Domain.Users;

public sealed partial class Phone : SimpleValueObject<string>
{
    public const int NationalNumberLength = 9;   // 99 628 66 44
    public const int MaxLength = 13;             // +380 + 9 цифр

    private const string CountryCode = "380";
    private const string CountryPrefix = "+380";
    private const int OperatorCodeLength = 2;

    private Phone(string value) : base(value) { }

    public static Result<Phone, Error> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DomainErrors.Users.PhoneIsRequired();

        string trimmed = value.Trim();

        if (!AllowedCharacters().IsMatch(trimmed))
            return DomainErrors.Users.PhoneInvalidFormat();

        string digits = NonDigits().Replace(trimmed, string.Empty);

        string? national = ExtractNationalNumber(digits);

        if (national is null)
            return DomainErrors.Users.PhoneInvalidLength(NationalNumberLength);

        if (!IsAllocatedMobileCode(national.AsSpan(0, OperatorCodeLength)))
            return DomainErrors.Users.PhoneUnknownOperatorCode();

        return new Phone(CountryPrefix + national);
    }

    private static string? ExtractNationalNumber(string digits) => digits.Length switch
    {
        NationalNumberLength => digits,
        10 when digits[0] == '0' => digits[1..],
        12 when digits.StartsWith(CountryCode, StringComparison.Ordinal)
            => digits[CountryCode.Length..],
        _ => null
    };

    /// <summary>
    /// Мобильные коды, выделенные НКЕК. Проверяется именно факт выделения диапазона, а не то,
    /// кто его обслуживает: с 2019 года номер переносят между операторами вместе с кодом,
    /// поэтому по коду оператора больше не узнать — и узнавать не нужно.
    /// <para>
    /// Городских кодов здесь нет намеренно. Телефон у нас служит логином и способом дозвониться
    /// по заказу, а нужен тот номер, который человек носит с собой, а не тот, что стоит в коридоре.
    /// </para>
    /// <para>
    /// Список меняется раз в несколько лет — тогда правится эта строка. Внешний справочник не берём:
    /// проверка кода не стоит того, чтобы регистрация падала вместе с чужим сервисом.
    /// </para>
    /// </summary>
    private static bool IsAllocatedMobileCode(ReadOnlySpan<char> code) => code switch
    {
        "39" or "50" or "63" or "66" or "67" or "68" or "73" or "91"
            or "92" or "93" or "94" or "95" or "96" or "97" or "98" or "99" => true,
        _ => false
    };

    [GeneratedRegex(@"^\+?[0-9\s()-]+$")]
    private static partial Regex AllowedCharacters();

    [GeneratedRegex(@"[^0-9]")]
    private static partial Regex NonDigits();
}
