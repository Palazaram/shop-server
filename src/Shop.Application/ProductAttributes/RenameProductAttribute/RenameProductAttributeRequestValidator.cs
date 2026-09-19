using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Errors;
using Shop.Domain.ProductAttributes;

namespace Shop.Application.ProductAttributes.RenameProductAttribute;

public sealed class RenameProductAttributeRequestValidator
    : AbstractValidator<RenameProductAttributeRequest>
{
    public RenameProductAttributeRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty()
                .WithError(DomainErrors.ProductAttributes.NameIsRequired())
            .MaximumLength(ProductAttribute.MaxNameLength)
                .WithError(DomainErrors.ProductAttributes.NameTooLong(ProductAttribute.MaxNameLength));
    }
}
