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

    [GeneratedRegex(@"^\+?[0-9\s()-]+$")]
    private static partial Regex AllowedCharacters();

    [GeneratedRegex(@"[^0-9]")]
    private static partial Regex NonDigits();
}