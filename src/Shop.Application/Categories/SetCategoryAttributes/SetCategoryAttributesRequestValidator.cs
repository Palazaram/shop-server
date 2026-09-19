using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Errors;

namespace Shop.Application.Categories.SetCategoryAttributes;

public sealed class SetCategoryAttributesRequestValidator
    : AbstractValidator<SetCategoryAttributesRequest>
{
    public SetCategoryAttributesRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.AttributeIds)
            .NotNull()
                .WithError(DomainErrors.Categories.AttributeIdsAreRequired())
            .Must(ids => ids!.All(id => id != Guid.Empty))
                .WithError(DomainErrors.Categories.AttributeIdIsInvalid())
            .Must(ids => ids!.Distinct().Count() == ids!.Count)
                .WithError(DomainErrors.Categories.DuplicateAttribute());
    }
}