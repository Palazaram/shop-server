using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Errors;
using Shop.Domain.Manufacturers;

namespace Shop.Application.Manufacturers.UpdateManufacturer;

public sealed class UpdateManufacturerRequestValidator
    : AbstractValidator<UpdateManufacturerRequest>
{
    public UpdateManufacturerRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty()
                .WithError(DomainErrors.Manufacturers.NameIsRequired())
            .MaximumLength(Manufacturer.MaxNameLength)
                .WithError(DomainErrors.Manufacturers.NameTooLong(Manufacturer.MaxNameLength));

        RuleFor(x => x.Country)
            .NotEmpty()
                .WithError(DomainErrors.Manufacturers.CountryIsRequired())
            .MaximumLength(Manufacturer.MaxCountryLength)
                .WithError(DomainErrors.Manufacturers.CountryTooLong(Manufacturer.MaxCountryLength));
    }
}