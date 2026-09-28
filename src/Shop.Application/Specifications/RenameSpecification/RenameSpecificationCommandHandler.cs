using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;
using Shop.Domain.Specifications;

namespace Shop.Application.Specifications.RenameSpecification;

internal sealed class RenameSpecificationCommandHandler(
    ISpecificationRepository specificationRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<RenameSpecificationCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        RenameSpecificationCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<Specification> maybeSpecification =
            await specificationRepository.GetByIdAsync(command.SpecificationId, cancellationToken);

        if (maybeSpecification.HasNoValue)
            return DomainErrors.Specifications.NotFound();

        Specification specification = maybeSpecification.Value;

        UnitResult<Error> renamed = specification.Rename(command.Name);
        if (renamed.IsFailure)
            return renamed.Error;

        if (await specificationRepository.ExistsByNameAsync(
                specification.Name, specification.Id, cancellationToken))
            return DomainErrors.Specifications.NameAlreadyExists();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}
