using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Common;

namespace Shop.Application.Countries.ChangeCountrySlug;

public sealed class ChangeCountrySlugRequestValidator : AbstractValidator<ChangeCountrySlugRequest>
{
    public ChangeCountrySlugRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Slug)
            .MustBeValueObject(Slug.Create);
    }
}