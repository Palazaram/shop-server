using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Errors;
using Shop.Domain.Specifications;

namespace Shop.Application.Specifications.CreateSpecification;

public sealed class CreateSpecificationCommandValidator
    : AbstractValidator<CreateSpecificationCommand>
{
    public CreateSpecificationCommandValidator()
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
