using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Countries;
using Shop.Domain.Errors;

namespace Shop.Application.Countries.CreateCountry;

internal sealed class CreateCountryCommandHandler(
    ICountryRepository countryRepository,
    ISlugGenerator slugGenerator,
    IUnitOfWork unitOfWork)
        : ICommandHandler<CreateCountryCommand, CreateCountryResponse>
{
    public async Task<Result<CreateCountryResponse, Error>> HandleAsync(
        CreateCountryCommand command,
        CancellationToken cancellationToken)
    {
        bool slugProvided = !string.IsNullOrWhiteSpace(command.Slug);

        string slugSource = slugProvided
            ? command.Slug!
            : slugGenerator.Generate(command.Name);

        Result<Slug, Error> slugResult = Slug.Create(slugSource);
        if (slugResult.IsFailure)
            return slugProvided
                ? slugResult.Error
                : DomainErrors.Countries.SlugCannotBeGenerated();

        Result<Country, Error> countryResult = Country.Create(command.Name, slugResult.Value);
        if (countryResult.IsFailure)
            return countryResult.Error;

        Country country = countryResult.Value;

        if (await countryRepository.ExistsByNameAsync(country.Name, null, cancellationToken))
            return DomainErrors.Countries.NameAlreadyExists();

        if (await countryRepository.ExistsBySlugAsync(country.Slug, null, cancellationToken))
            return DomainErrors.Countries.SlugAlreadyExists();

        countryRepository.Add(country);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateCountryResponse(country.Id, country.Slug.Value);
    }
}