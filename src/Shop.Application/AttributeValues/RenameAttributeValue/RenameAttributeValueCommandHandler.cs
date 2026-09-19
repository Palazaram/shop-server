using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.AttributeValues;
using Shop.Domain.Errors;

namespace Shop.Application.AttributeValues.RenameAttributeValue;

internal sealed class RenameAttributeValueCommandHandler(
    IAttributeValueRepository valueRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RenameAttributeValueCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        RenameAttributeValueCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<AttributeValue> maybeValue =
            await valueRepository.GetByIdAsync(command.ValueId, cancellationToken);

        if (maybeValue.HasNoValue)
            return DomainErrors.AttributeValues.NotFound();

        AttributeValue value = maybeValue.Value;

        UnitResult<Error> renameResult = value.Rename(command.Name);
        if (renameResult.IsFailure)
            return renameResult;

        if (await valueRepository.ExistsByNameAsync(
                value.AttributeId, value.Name, value.Id, cancellationToken))
            return DomainErrors.AttributeValues.NameAlreadyExists();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}