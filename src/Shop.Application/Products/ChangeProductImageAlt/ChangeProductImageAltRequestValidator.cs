using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Errors;
using Shop.Domain.Products;

namespace Shop.Application.Products.ChangeProductImageAlt;

public sealed class ChangeProductImageAltRequestValidator
    : AbstractValidator<ChangeProductImageAltRequest>
{
    public ChangeProductImageAltRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Alt)
            .MaximumLength(ProductImage.MaxAltLength)
                .WithError(DomainErrors.Products.ImageAltTooLong(ProductImage.MaxAltLength));
    }
}