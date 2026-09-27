using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Common;

namespace Shop.Application.ProductAttributes.ChangeProductAttributeSlug;

public sealed class ChangeProductAttributeSlugRequestValidator
    : AbstractValidator<ChangeProductAttributeSlugRequest>
{
    public ChangeProductAttributeSlugRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Slug)
            .MustBeValueObject(Slug.Create);
    }
}