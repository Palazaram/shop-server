using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Errors;

namespace Shop.Application.Products.SetProductFeatured;

public sealed class SetProductFeaturedRequestValidator
    : AbstractValidator<SetProductFeaturedRequest>
{
    public SetProductFeaturedRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.IsFeatured)
            .NotNull()
                .WithError(DomainErrors.Products.IsFeaturedIsRequired());
    }
}
