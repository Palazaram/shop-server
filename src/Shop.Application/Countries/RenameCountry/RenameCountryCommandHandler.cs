using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Countries;
using Shop.Domain.Errors;

namespace Shop.Application.Countries.RenameCountry;

internal sealed class RenameCountryCommandHandler(
    ICountryRepository countryRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<RenameCountryCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        RenameCountryCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<Country> maybeCountry =
            await countryRepository.GetByIdAsync(command.CountryId, cancellationToken);

        if (maybeCountry.HasNoValue)
            return DomainErrors.Countries.NotFound();

        Country country = maybeCountry.Value;

        UnitResult<Error> renameResult = country.Rename(command.Name);
        if (renameResult.IsFailure)
            return renameResult;

        // Нормализация имени приватна, поэтому уникальность проверяется после переименования.
        if (await countryRepository.ExistsByNameAsync(country.Name, country.Id, cancellationToken))
            return DomainErrors.Countries.NameAlreadyExists();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}