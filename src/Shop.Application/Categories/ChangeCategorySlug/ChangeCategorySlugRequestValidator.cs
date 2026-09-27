using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Common;

namespace Shop.Application.Categories.ChangeCategorySlug;

public sealed class ChangeCategorySlugRequestValidator
    : AbstractValidator<ChangeCategorySlugRequest>
{
    public ChangeCategorySlugRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Slug)
            .MustBeValueObject(Slug.Create);
    }
}