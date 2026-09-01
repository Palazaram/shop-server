using CSharpFunctionalExtensions;
using Shop.Domain.Errors;
using System.Text.RegularExpressions;

namespace Shop.Domain.Users;

public sealed partial class PersonName : SimpleValueObject<string>
{
    public const int MinLength = 2;
    public const int MaxLength = 50;

    // Строго украинский алфавит + апостроф, дефис и пробел (Мар'яна, Марія-Анна)
    [GeneratedRegex(@"^[АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮЯабвгґдеєжзиіїйклмнопрстуфхцчшщьюя'\- ]+$")]
    private static partial Regex Format();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRuns();

    private PersonName(string value) : base(value) { }

    public static Result<PersonName, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DomainErrors.Users.NameIsRequired();

        // Trim + схлопывание внутренних пробелов + типографский апостроф → обычный.
        // Регистр НЕ трогаем: имя — имя собственное.
        string normalized = WhitespaceRuns()
            .Replace(value.Trim(), " ")
            .Replace('\u2019', '\'');

        if (normalized.Length < MinLength)
            return DomainErrors.Users.NameTooShort(MinLength);

        if (normalized.Length > MaxLength)
            return DomainErrors.Users.NameTooLong(MaxLength);

        if (!Format().IsMatch(normalized))
            return DomainErrors.Users.NameInvalidFormat();

        return new PersonName(normalized);
    }
}