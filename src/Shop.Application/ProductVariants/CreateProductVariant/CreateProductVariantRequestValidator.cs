using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Common;
using Shop.Domain.Errors;
using Shop.Domain.ProductVariants;

namespace Shop.Application.ProductVariants.CreateProductVariant;

public sealed class CreateProductVariantRequestValidator
    : AbstractValidator<CreateProductVariantRequest>
{
    public CreateProductVariantRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Sku)
            .NotEmpty()
                .WithError(DomainErrors.ProductVariants.SkuIsRequired())
            .MaximumLength(ProductVariant.MaxSkuLength)
                .WithError(DomainErrors.ProductVariants.SkuTooLong(ProductVariant.MaxSkuLength));

        RuleFor(x => x.PackagingValue)
            .NotNull()
                .WithError(DomainErrors.Packagings.ValueIsRequired())
            .GreaterThan(0)
                .WithError(DomainErrors.Packagings.ValueMustBePositive());

        RuleFor(x => x.PackagingUnit)
            .NotNull()
                .WithError(DomainErrors.Packagings.UnitIsRequired())
            .IsInEnum()
                .WithError(DomainErrors.Packagings.UnitIsInvalid());

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

        RuleFor(x => x.Slug)
            .MustBeValueObject(Slug.Create)
            .When(x => !string.IsNullOrWhiteSpace(x.Slug));
    }
}