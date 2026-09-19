using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Common;
using Shop.Domain.Errors;
using Shop.Domain.ProductAttributes;

namespace Shop.Application.ProductAttributes.CreateProductAttribute;

public sealed class CreateProductAttributeCommandValidator
    : AbstractValidator<CreateProductAttributeCommand>
{
    public CreateProductAttributeCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty()
                .WithError(DomainErrors.ProductAttributes.NameIsRequired())
            .MaximumLength(ProductAttribute.MaxNameLength)
                .WithError(DomainErrors.ProductAttributes.NameTooLong(ProductAttribute.MaxNameLength));

        RuleFor(x => x.Slug)
            .MustBeValueObject(Slug.Create)
            .When(x => !string.IsNullOrWhiteSpace(x.Slug));
    }
}