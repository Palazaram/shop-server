using CSharpFunctionalExtensions;
using Shop.Domain.Errors;

namespace Shop.Domain.Users;

public sealed class PasswordHash : SimpleValueObject<string>
{
    private PasswordHash(string value) : base(value) { }

    public static Result<PasswordHash, Error> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DomainErrors.Users.PasswordHashIsRequired();

        return new PasswordHash(value);
    }
}
