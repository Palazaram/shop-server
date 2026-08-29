using CSharpFunctionalExtensions;
using Shop.Domain.Errors;
using System.Text.RegularExpressions;

namespace Shop.Domain.Users;

public sealed partial class FullName : ValueObject
{
    public const int MaxLength = 50;
    public string FirstName { get; }
    public string LastName { get; }
    public string Patronymic { get; }

    // Строго украинский алфавит + апостроф, дефис и пробел (Мар'яна, Марія-Анна)
    [GeneratedRegex(@"^[АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЬЮЯабвгґдеєжзиіїйклмнопрстуфхцчшщьюя'\- ]+$")]
    private static partial Regex Format();

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRuns();
    private const int MinLength = 2;

    private FullName(string firstName, string lastName, string patronymic)
    {
        FirstName = firstName;
        LastName = lastName;
        Patronymic = patronymic;
    }

    public static Result<FullName, Error> Create(string firstName, string lastName, string patronymic)
    {
        Result<string, Error> firstNameResult = ValidatePart(firstName, FirstNameErrors);
        if (firstNameResult.IsFailure)
            return firstNameResult.Error;

        Result<string, Error> lastNameResult = ValidatePart(lastName, LastNameErrors);
        if (lastNameResult.IsFailure)
            return lastNameResult.Error;

        Result<string, Error> patronymicResult = ValidatePart(patronymic, PatronymicErrors);
        if (patronymicResult.IsFailure)
            return patronymicResult.Error;

        return new FullName(
            firstNameResult.Value,
            lastNameResult.Value,
            patronymicResult.Value);
    }

    public string DisplayName() => $"{LastName} {FirstName} {Patronymic}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
        yield return Patronymic;
    }

    // --- валидация одной части имени ---

    private static Result<string, Error> ValidatePart(string value, NamePartErrors errors)
    {
        if (string.IsNullOrWhiteSpace(value))
            return errors.Required();

        // Trim + схлопывание внутренних пробелов + типографский апостроф → обычный.
        // Регистр НЕ трогаем: имя — имя собственное.
        string normalized = WhitespaceRuns()
            .Replace(value.Trim(), " ")
            .Replace('\u2019', '\'');

        if (normalized.Length < MinLength)
            return errors.TooShort(MinLength);

        if (normalized.Length > MaxLength)
            return errors.TooLong(MaxLength);

        if (!Format().IsMatch(normalized))
            return errors.InvalidFormat();

        return normalized;
    }

    private sealed record NamePartErrors(
        Func<Error> Required,
        Func<int, Error> TooShort,
        Func<int, Error> TooLong,
        Func<Error> InvalidFormat);

    private static readonly NamePartErrors FirstNameErrors = new(
        DomainErrors.Users.FirstNameIsRequired,
        DomainErrors.Users.FirstNameTooShort,
        DomainErrors.Users.FirstNameTooLong,
        DomainErrors.Users.FirstNameInvalidFormat);

    private static readonly NamePartErrors LastNameErrors = new(
        DomainErrors.Users.LastNameIsRequired,
        DomainErrors.Users.LastNameTooShort,
        DomainErrors.Users.LastNameTooLong,
        DomainErrors.Users.LastNameInvalidFormat);

    private static readonly NamePartErrors PatronymicErrors = new(
        DomainErrors.Users.PatronymicIsRequired,
        DomainErrors.Users.PatronymicTooShort,
        DomainErrors.Users.PatronymicTooLong,
        DomainErrors.Users.PatronymicInvalidFormat);
}