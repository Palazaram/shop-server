using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Errors;

namespace Shop.Application.Specifications.ReorderSpecifications;

public sealed class ReorderSpecificationsCommandValidator
    : AbstractValidator<ReorderSpecificationsCommand>
{
    public ReorderSpecificationsCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.SpecificationIds)
            .NotEmpty()
                .WithError(DomainErrors.Specifications.OrderIsRequired())
            .Must(ids => ids!.All(id => id != Guid.Empty))
                .WithError(DomainErrors.Specifications.IdIsInvalid())
            .Must(ids => ids!.Distinct().Count() == ids!.Count)
                .WithError(DomainErrors.Specifications.DuplicateInOrder());
    }
}
