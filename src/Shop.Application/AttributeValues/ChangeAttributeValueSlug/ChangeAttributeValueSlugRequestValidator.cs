using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Common;

namespace Shop.Application.AttributeValues.ChangeAttributeValueSlug;

public sealed class ChangeAttributeValueSlugRequestValidator
    : AbstractValidator<ChangeAttributeValueSlugRequest>
{
    public ChangeAttributeValueSlugRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Slug)
            .MustBeValueObject(Slug.Create);
    }
}