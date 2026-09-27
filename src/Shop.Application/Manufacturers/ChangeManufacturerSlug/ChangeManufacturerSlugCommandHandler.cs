using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Errors;
using Shop.Domain.Manufacturers;

namespace Shop.Application.Manufacturers.ChangeManufacturerSlug;

internal sealed class ChangeManufacturerSlugCommandHandler(
    IManufacturerRepository manufacturerRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<ChangeManufacturerSlugCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        ChangeManufacturerSlugCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<Manufacturer> maybeManufacturer =
            await manufacturerRepository.GetByIdAsync(command.ManufacturerId, cancellationToken);

        if (maybeManufacturer.HasNoValue)
            return DomainErrors.Manufacturers.NotFound();

        Result<Slug, Error> slugResult = Slug.Create(command.Slug);
        if (slugResult.IsFailure)
            return slugResult.Error;

        Manufacturer manufacturer = maybeManufacturer.Value;

        if (await manufacturerRepository.ExistsBySlugAsync(
                slugResult.Value, manufacturer.Id, cancellationToken))
            return DomainErrors.Manufacturers.SlugAlreadyExists();

        manufacturer.ChangeSlug(slugResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}