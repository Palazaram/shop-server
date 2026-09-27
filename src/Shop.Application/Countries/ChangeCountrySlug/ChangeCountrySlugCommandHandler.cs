using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Countries;
using Shop.Domain.Errors;

namespace Shop.Application.Countries.ChangeCountrySlug;

internal sealed class ChangeCountrySlugCommandHandler(
    ICountryRepository countryRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<ChangeCountrySlugCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        ChangeCountrySlugCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<Country> maybeCountry =
            await countryRepository.GetByIdAsync(command.CountryId, cancellationToken);

        if (maybeCountry.HasNoValue)
            return DomainErrors.Countries.NotFound();

        Result<Slug, Error> slugResult = Slug.Create(command.Slug);
        if (slugResult.IsFailure)
            return slugResult.Error;

        Country country = maybeCountry.Value;

        if (await countryRepository.ExistsBySlugAsync(
                slugResult.Value, country.Id, cancellationToken))
            return DomainErrors.Countries.SlugAlreadyExists();

        country.ChangeSlug(slugResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}