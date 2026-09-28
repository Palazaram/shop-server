using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Errors;
using Shop.Domain.Specifications;

namespace Shop.Application.Specifications.RenameSpecification;

public sealed class RenameSpecificationRequestValidator
    : AbstractValidator<RenameSpecificationRequest>
{
    public RenameSpecificationRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty()
                .WithError(DomainErrors.Specifications.NameIsRequired())
            .MaximumLength(Specification.MaxNameLength)
                .WithError(DomainErrors.Specifications.NameTooLong(Specification.MaxNameLength));
    }
}
