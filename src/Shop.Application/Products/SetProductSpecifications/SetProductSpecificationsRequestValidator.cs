using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Errors;
using Shop.Domain.Products;

namespace Shop.Application.Products.SetProductSpecifications;

public sealed class SetProductSpecificationsRequestValidator
    : AbstractValidator<SetProductSpecificationsRequest>
{
    public SetProductSpecificationsRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        // Пустой список — законное «снять все», как у значений атрибутов, поэтому NotNull,
        // а не NotEmpty.
        RuleFor(x => x.Specifications)
            .NotNull()
                .WithError(DomainErrors.Products.SpecificationsAreRequired())
            .Must(items => items!.All(item => item.SpecificationId is Guid id && id != Guid.Empty))
                .WithError(DomainErrors.Products.SpecificationIdIsInvalid())
            .Must(items => items!
                .Select(item => item.SpecificationId)
                .Distinct()
                .Count() == items!.Count)
                .WithError(DomainErrors.Products.DuplicateSpecification())
            .Must(items => items!.All(item => !string.IsNullOrWhiteSpace(item.Value)))
                .WithError(DomainErrors.Products.SpecificationValueIsRequired())
            .Must(items => items!.All(item =>
                item.Value!.Trim().Length <= ProductSpecification.MaxValueLength))
                .WithError(DomainErrors.Products.SpecificationValueTooLong(
                    ProductSpecification.MaxValueLength));
    }
}
