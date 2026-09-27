using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Common;
using Shop.Domain.Countries;
using Shop.Domain.Errors;

namespace Shop.Application.Countries.CreateCountry;

public sealed class CreateCountryCommandValidator : AbstractValidator<CreateCountryCommand>
{
    public CreateCountryCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty()
                .WithError(DomainErrors.Countries.NameIsRequired())
            .MaximumLength(Country.MaxNameLength)
                .WithError(DomainErrors.Countries.NameTooLong(Country.MaxNameLength));

        RuleFor(x => x.Slug)
            .MustBeValueObject(Slug.Create)
            .When(x => !string.IsNullOrWhiteSpace(x.Slug));
    }
}