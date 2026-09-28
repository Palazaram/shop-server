using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;
using Shop.Domain.Specifications;

namespace Shop.Application.Specifications.CreateSpecification;

internal sealed class CreateSpecificationCommandHandler(
    ISpecificationRepository specificationRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<CreateSpecificationCommand, CreateSpecificationResponse>
{
    public async Task<Result<CreateSpecificationResponse, Error>> HandleAsync(
        CreateSpecificationCommand command,
        CancellationToken cancellationToken)
    {
        // Новая характеристика встаёт последней: порядок задаёт отдельная команда.
        int displayOrder = await specificationRepository.GetNextDisplayOrderAsync(cancellationToken);

        Result<Specification, Error> result = Specification.Create(command.Name, displayOrder);

        if (result.IsFailure)
            return result.Error;

        Specification specification = result.Value;

        if (await specificationRepository.ExistsByNameAsync(
                specification.Name, null, cancellationToken))
            return DomainErrors.Specifications.NameAlreadyExists();

        specificationRepository.Add(specification);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateSpecificationResponse(specification.Id);
    }
}
