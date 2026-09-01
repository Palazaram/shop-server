using CSharpFunctionalExtensions;
using Shop.Domain.Errors;

namespace Shop.Domain.Users;

public sealed class FullName : ValueObject
{
    public PersonName FirstName { get; }
    public PersonName LastName { get; }
    public PersonName Patronymic { get; }

    private FullName(PersonName firstName, PersonName lastName, PersonName patronymic)
    {
        FirstName = firstName;
        LastName = lastName;
        Patronymic = patronymic;
    }

    public static Result<FullName, Error> Create(string firstName, string lastName, string patronymic)
    {
        var firstNameResult = PersonName.Create(firstName);
        if (firstNameResult.IsFailure)
            return firstNameResult.Error;

        var lastNameResult = PersonName.Create(lastName);
        if (lastNameResult.IsFailure)
            return lastNameResult.Error;

        var patronymicResult = PersonName.Create(patronymic);
        if (patronymicResult.IsFailure)
            return patronymicResult.Error;

        return new FullName(
            firstNameResult.Value,
            lastNameResult.Value,
            patronymicResult.Value);
    }

    public string DisplayName() => $"{LastName.Value} {FirstName.Value} {Patronymic.Value}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
        yield return Patronymic;
    }
}