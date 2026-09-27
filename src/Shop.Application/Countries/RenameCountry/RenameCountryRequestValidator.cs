using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Countries;
using Shop.Domain.Errors;

namespace Shop.Application.Countries.RenameCountry;

public sealed class RenameCountryRequestValidator : AbstractValidator<RenameCountryRequest>
{
    public RenameCountryRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty()
                .WithError(DomainErrors.Countries.NameIsRequired())
            .MaximumLength(Country.MaxNameLength)
                .WithError(DomainErrors.Countries.NameTooLong(Country.MaxNameLength));
    }
}