using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Categories;
using Shop.Domain.Errors;
using Shop.Domain.ProductAttributes;

namespace Shop.Application.Categories.SetCategoryAttributes;

internal sealed class SetCategoryAttributesCommandHandler(
    ICategoryRepository categoryRepository,
    IProductAttributeRepository attributeRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SetCategoryAttributesCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        SetCategoryAttributesCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<Category> maybeCategory =
            await categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);

        if (maybeCategory.HasNoValue)
            return DomainErrors.Categories.NotFound();

        IReadOnlyList<Guid> attributeIds = command.AttributeIds!;

        if (!await attributeRepository.AllExistAsync(attributeIds, cancellationToken))
            return DomainErrors.Categories.AttributeNotFound();

        UnitResult<Error> result = maybeCategory.Value.SetAttributes(attributeIds);
        if (result.IsFailure)
            return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}