using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;
using Shop.Domain.Manufacturers;

namespace Shop.Application.Manufacturers.UpdateManufacturer;

internal sealed class UpdateManufacturerCommandHandler(
    IManufacturerRepository manufacturerRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<UpdateManufacturerCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        UpdateManufacturerCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<Manufacturer> maybeManufacturer =
            await manufacturerRepository.GetByIdAsync(command.ManufacturerId, cancellationToken);

        if (maybeManufacturer.HasNoValue)
            return DomainErrors.Manufacturers.NotFound();

        Manufacturer manufacturer = maybeManufacturer.Value;

        UnitResult<Error> renameResult = manufacturer.Rename(command.Name);
        if (renameResult.IsFailure)
            return renameResult;

        UnitResult<Error> countryResult = manufacturer.ChangeCountry(command.Country);
        if (countryResult.IsFailure)
            return countryResult;

        if (await manufacturerRepository.ExistsByNameAsync(
                manufacturer.Name, manufacturer.Id, cancellationToken))
            return DomainErrors.Manufacturers.NameAlreadyExists();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}