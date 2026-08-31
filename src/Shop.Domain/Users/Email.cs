using CSharpFunctionalExtensions;
using Shop.Domain.Errors;
using System.Net.Mail;

namespace Shop.Domain.Users;

public sealed class Email : SimpleValueObject<string>
{
    public const int MaxLength = 254;

    private Email(string value) : base(value) { }

    public static Result<Email, Error> Create(string value) 
    {
        if (string.IsNullOrWhiteSpace(value))
            return DomainErrors.Users.EmailIsRequired();

        string normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > MaxLength)
            return DomainErrors.Users.EmailTooLong(MaxLength);

        if (!MailAddress.TryCreate(normalized, out MailAddress? email))
            return DomainErrors.Users.EmailInvalidFormat();

        string host = email.Host;
        int lastDot = host.LastIndexOf('.');

        if (lastDot < 1 || host.Length - lastDot - 1 < 2)
            return DomainErrors.Users.EmailInvalidFormat();

        if (!string.Equals(email.Address, normalized, StringComparison.Ordinal))
            return DomainErrors.Users.EmailInvalidFormat();

        return new Email(email.Address);
    }
}