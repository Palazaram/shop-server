using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Errors;

namespace Shop.Application.Users.ChangePassword;

public sealed class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        // Текущий пароль здесь только на «не пусто»: требования к сложности могли измениться
        // с тех пор, как его завели, и придираться к нему поздно.
        RuleFor(x => x.CurrentPassword)
            .NotEmpty()
                .WithError(DomainErrors.Users.PasswordIsRequired());

        RuleFor(x => x.NewPassword).ValidPassword();
    }
}
