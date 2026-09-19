using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;
using Shop.Domain.ProductAttributes;

namespace Shop.Application.ProductAttributes.RenameProductAttribute;

internal sealed class RenameProductAttributeCommandHandler(
    IProductAttributeRepository attributeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<RenameProductAttributeCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        RenameProductAttributeCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<ProductAttribute> maybeAttribute =
            await attributeRepository.GetByIdAsync(command.AttributeId, cancellationToken);

        if (maybeAttribute.HasNoValue)
            return DomainErrors.ProductAttributes.NotFound();

        ProductAttribute attribute = maybeAttribute.Value;

        UnitResult<Error> renameResult = attribute.Rename(command.Name);
        if (renameResult.IsFailure)
            return renameResult;

        if (await attributeRepository.ExistsByNameAsync(
                attribute.Name, attribute.Id, cancellationToken))
            return DomainErrors.ProductAttributes.NameAlreadyExists();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}