using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Errors;
using Shop.Domain.ProductVariants;

namespace Shop.Application.ProductVariants.UpdateProductVariant;

public sealed class UpdateProductVariantRequestValidator
    : AbstractValidator<UpdateProductVariantRequest>
{
    public UpdateProductVariantRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Sku)
            .NotEmpty()
                .WithError(DomainErrors.ProductVariants.SkuIsRequired())
            .MaximumLength(ProductVariant.MaxSkuLength)
                .WithError(DomainErrors.ProductVariants.SkuTooLong(ProductVariant.MaxSkuLength));

        RuleFor(x => x.Price)
            .NotNull()
                .WithError(DomainErrors.Money.IsRequired())
            .GreaterThan(0)
                .WithError(DomainErrors.Money.MustBePositive());

        RuleFor(x => x.StockQuantity)
            .NotNull()
                .WithError(DomainErrors.ProductVariants.StockIsRequired())
            .GreaterThanOrEqualTo(0)
                .WithError(DomainErrors.ProductVariants.StockMustNotBeNegative());
    }
}