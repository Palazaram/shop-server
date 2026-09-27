using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Common;

namespace Shop.Application.Manufacturers.ChangeManufacturerSlug;

public sealed class ChangeManufacturerSlugRequestValidator
    : AbstractValidator<ChangeManufacturerSlugRequest>
{
    public ChangeManufacturerSlugRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Slug)
            .MustBeValueObject(Slug.Create);
    }
}