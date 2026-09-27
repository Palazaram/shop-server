using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Common;

namespace Shop.Application.ProductVariants.ChangeProductVariantSlug;

public sealed class ChangeProductVariantSlugRequestValidator
    : AbstractValidator<ChangeProductVariantSlugRequest>
{
    public ChangeProductVariantSlugRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Slug)
            .MustBeValueObject(Slug.Create);
    }
}