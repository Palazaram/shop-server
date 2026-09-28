using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Errors;
using Shop.Domain.Products;

namespace Shop.Application.Products.SetProductSeo;

public sealed class SetProductSeoRequestValidator : AbstractValidator<SetProductSeoRequest>
{
    public SetProductSeoRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        // Оба поля необязательны: null и пустая строка означают «не заполнено».
        RuleFor(x => x.MetaTitle)
            .MaximumLength(Product.MaxMetaTitleLength)
                .WithError(DomainErrors.Products.MetaTitleTooLong(
                    Product.MaxMetaTitleLength));

        RuleFor(x => x.MetaDescription)
            .MaximumLength(Product.MaxMetaDescriptionLength)
                .WithError(DomainErrors.Products.MetaDescriptionTooLong(
                    Product.MaxMetaDescriptionLength));
    }
}
