using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Errors;
using Shop.Domain.Products;

namespace Shop.Application.Products.UpdateProduct;

public sealed class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
{
    public UpdateProductRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty()
                .WithError(DomainErrors.Products.NameIsRequired())
            .MaximumLength(Product.MaxNameLength)
                .WithError(DomainErrors.Products.NameTooLong(Product.MaxNameLength));

        RuleFor(x => x.Description)
            .MaximumLength(Product.MaxDescriptionLength)
                .WithError(DomainErrors.Products.DescriptionTooLong(Product.MaxDescriptionLength));

        RuleFor(x => x.CategoryId)
            .NotEmpty()
                .WithError(DomainErrors.Products.CategoryIdIsInvalid());

        RuleFor(x => x.ManufacturerId)
            .NotEmpty()
                .WithError(DomainErrors.Products.ManufacturerIdIsInvalid());
    }
}