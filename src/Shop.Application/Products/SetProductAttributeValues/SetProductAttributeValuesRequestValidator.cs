using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Errors;

namespace Shop.Application.Products.SetProductAttributeValues;

public sealed class SetProductAttributeValuesRequestValidator
    : AbstractValidator<SetProductAttributeValuesRequest>
{
    public SetProductAttributeValuesRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ValueIds)
            .NotNull()
                .WithError(DomainErrors.Products.AttributeValueIdsAreRequired())
            .Must(ids => ids!.All(id => id != Guid.Empty))
                .WithError(DomainErrors.Products.AttributeValueIdIsInvalid())
            .Must(ids => ids!.Distinct().Count() == ids!.Count)
                .WithError(DomainErrors.Products.DuplicateAttributeValue());
    }
}