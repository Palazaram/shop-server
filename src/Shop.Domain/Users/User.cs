using CSharpFunctionalExtensions;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;

namespace Shop.Domain.Users;

public sealed class User : AggregateRoot<Guid>
{
    public FullName FullName { get; private set; } = default!;
    public Phone Phone { get; private set; } = default!;
    public PasswordHash PasswordHash { get; private set; } = default!;
    public Email Email { get; private set; } = default!;
    public int RoleId { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private User() { }

    private User(
        Guid id,
        FullName fullName,
        Phone phone,
        PasswordHash passwordHash,
        Email email,
        int roleId,
        DateTimeOffset createdAt)
    {
        Id = id;
        FullName = fullName;
        Phone = phone;
        PasswordHash = passwordHash;
        Email = email;
        RoleId = roleId;
        CreatedAt = createdAt;
    }

    public static User Create(
        FullName fullName,
        Phone phone,
        PasswordHash passwordHash,
        Email email,
        int roleId,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(fullName);
        ArgumentNullException.ThrowIfNull(phone);
        ArgumentNullException.ThrowIfNull(passwordHash);
        ArgumentNullException.ThrowIfNull(email);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roleId);

        return new User(
            Guid.CreateVersion7(),
            fullName,
            phone,
            passwordHash,
            email,
            roleId,
            createdAt);
    }

    public UnitResult<Error> ChangeEmail(Email email)
    {
        ArgumentNullException.ThrowIfNull(email);

        if (Email == email)
            return DomainErrors.Users.EmailAlreadySet();

        Email = email;

        return default;
    }

    public UnitResult<Error> ChangePhone(Phone phone)
    {
        ArgumentNullException.ThrowIfNull(phone);

        if (Phone == phone)
            return DomainErrors.Users.PhoneAlreadySet();

        Phone = phone;

        return default;
    }

    public UnitResult<Error> ChangeFullName(FullName fullName)
    {
        ArgumentNullException.ThrowIfNull(fullName);

        if (FullName == fullName)
            return DomainErrors.Users.FullNameAlreadySet();

        FullName = fullName;

        return default;
    }

    public void ChangePassword(PasswordHash newPasswordHash)
    {
        ArgumentNullException.ThrowIfNull(newPasswordHash);

        PasswordHash = newPasswordHash;
    }

    public UnitResult<Error> AssignRole(int roleId)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(roleId);

        if (RoleId == roleId)
            return DomainErrors.Users.RoleAlreadyAssigned();

        RoleId = roleId;

        return default;
    }
}