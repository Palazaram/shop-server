using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Errors;
using Shop.Domain.Users;

namespace Shop.Application.Users.LoginUser;

public sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;  // между полями — собираем все ошибки
        RuleLevelCascadeMode = CascadeMode.Stop;       // внутри поля — только первая

        RuleFor(x => x.Phone).MustBeValueObject(Phone.Create);

        RuleFor(x => x.Password).NotEmpty().WithError(DomainErrors.Users.PasswordIsRequired());
    }
}
