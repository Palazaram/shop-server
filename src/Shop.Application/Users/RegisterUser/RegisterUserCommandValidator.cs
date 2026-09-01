using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Users;

namespace Shop.Application.Users.RegisterUser;

public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;  // между полями — собираем все ошибки
        RuleLevelCascadeMode = CascadeMode.Stop;       // внутри поля — только первая

        RuleFor(x => x.FirstName).MustBeValueObject(PersonName.Create);
        RuleFor(x => x.LastName).MustBeValueObject(PersonName.Create);
        RuleFor(x => x.Patronymic).MustBeValueObject(PersonName.Create);

        RuleFor(x => x.Email).MustBeValueObject(Email.Create);
        RuleFor(x => x.Phone).MustBeValueObject(Phone.Create);

        RuleFor(x => x.Password).ValidPassword();
    }
}