using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Errors;
using Shop.Domain.Manufacturers;

namespace Shop.Application.Manufacturers.CreateManufacturer;

internal sealed class CreateManufacturerCommandHandler(
    IManufacturerRepository manufacturerRepository,
    ISlugGenerator slugGenerator,
    IUnitOfWork unitOfWork)
        : ICommandHandler<CreateManufacturerCommand, CreateManufacturerResponse>
{
    public async Task<Result<CreateManufacturerResponse, Error>> HandleAsync(
        CreateManufacturerCommand command,
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
                : DomainErrors.Manufacturers.SlugCannotBeGenerated();

        Result<Manufacturer, Error> manufacturerResult = Manufacturer.Create(
            name: command.Name,
            slug: slugResult.Value,
            country: command.Country);
        if (manufacturerResult.IsFailure)
            return manufacturerResult.Error;

        Manufacturer manufacturer = manufacturerResult.Value;

        if (await manufacturerRepository.ExistsByNameAsync(
                manufacturer.Name, null, cancellationToken))
            return DomainErrors.Manufacturers.NameAlreadyExists();

        if (await manufacturerRepository.ExistsBySlugAsync(manufacturer.Slug, cancellationToken))
            return DomainErrors.Manufacturers.SlugAlreadyExists();

        manufacturerRepository.Add(manufacturer);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateManufacturerResponse(manufacturer.Id, manufacturer.Slug.Value);
    }
}