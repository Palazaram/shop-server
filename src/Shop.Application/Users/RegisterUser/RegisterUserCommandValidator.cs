using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Errors;
using Shop.Domain.Users;

namespace Shop.Application.Users.RegisterUser;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;  // между полями — собираем все ошибки
        RuleLevelCascadeMode = CascadeMode.Stop;       // внутри поля — только первая

        RuleFor(x => x.FirstName)
            .NotEmpty().WithError(DomainErrors.Users.FirstNameIsRequired())
            .MaximumLength(FullName.MaxLength)
                .WithError(DomainErrors.Users.FirstNameTooLong(FullName.MaxLength));

        RuleFor(x => x.LastName)
            .NotEmpty().WithError(DomainErrors.Users.LastNameIsRequired())
            .MaximumLength(FullName.MaxLength)
                .WithError(DomainErrors.Users.LastNameTooLong(FullName.MaxLength));

        RuleFor(x => x.Patronymic)
            .NotEmpty().WithError(DomainErrors.Users.PatronymicIsRequired())
            .MaximumLength(FullName.MaxLength)
                .WithError(DomainErrors.Users.PatronymicTooLong(FullName.MaxLength));

        RuleFor(x => x.Email).MustBeValueObject(Email.Create);

        RuleFor(x => x.Phone).MustBeValueObject(Phone.Create);

        RuleFor(x => x.Password).ValidPassword();
    }
}