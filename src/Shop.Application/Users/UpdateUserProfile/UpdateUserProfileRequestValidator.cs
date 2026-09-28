using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Users;

namespace Shop.Application.Users.UpdateUserProfile;

public sealed class UpdateUserProfileRequestValidator
    : AbstractValidator<UpdateUserProfileRequest>
{
    public UpdateUserProfileRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.FirstName).MustBeValueObject(PersonName.Create);
        RuleFor(x => x.LastName).MustBeValueObject(PersonName.Create);
        RuleFor(x => x.Patronymic).MustBeValueObject(PersonName.Create);

        RuleFor(x => x.Phone).MustBeValueObject(Phone.Create);
    }
}
