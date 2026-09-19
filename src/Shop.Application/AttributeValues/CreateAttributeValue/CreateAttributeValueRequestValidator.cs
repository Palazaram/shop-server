using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.AttributeValues;
using Shop.Domain.Common;
using Shop.Domain.Errors;


namespace Shop.Application.AttributeValues.CreateAttributeValue;

public sealed class CreateAttributeValueRequestValidator
    : AbstractValidator<CreateAttributeValueRequest>
{
    public CreateAttributeValueRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty()
                .WithError(DomainErrors.AttributeValues.NameIsRequired())
            .MaximumLength(AttributeValue.MaxNameLength)
                .WithError(DomainErrors.AttributeValues.NameTooLong(AttributeValue.MaxNameLength));

        RuleFor(x => x.Slug)
            .MustBeValueObject(Slug.Create)
            .When(x => !string.IsNullOrWhiteSpace(x.Slug));
    }
}
