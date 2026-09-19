using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.AttributeValues;
using Shop.Domain.Errors;

namespace Shop.Application.AttributeValues.RenameAttributeValue;

public sealed class RenameAttributeValueRequestValidator
    : AbstractValidator<RenameAttributeValueRequest>
{
    public RenameAttributeValueRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty()
                .WithError(DomainErrors.AttributeValues.NameIsRequired())
            .MaximumLength(AttributeValue.MaxNameLength)
                .WithError(DomainErrors.AttributeValues.NameTooLong(AttributeValue.MaxNameLength));
    }
}
