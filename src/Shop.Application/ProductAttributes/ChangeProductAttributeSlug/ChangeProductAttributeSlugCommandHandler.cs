using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Errors;
using Shop.Domain.ProductAttributes;

namespace Shop.Application.ProductAttributes.ChangeProductAttributeSlug;

internal sealed class ChangeProductAttributeSlugCommandHandler(
    IProductAttributeRepository attributeRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<ChangeProductAttributeSlugCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        ChangeProductAttributeSlugCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<ProductAttribute> maybeAttribute =
            await attributeRepository.GetByIdAsync(command.AttributeId, cancellationToken);

        if (maybeAttribute.HasNoValue)
            return DomainErrors.ProductAttributes.NotFound();

        Result<Slug, Error> slugResult = Slug.Create(command.Slug);
        if (slugResult.IsFailure)
            return slugResult.Error;

        ProductAttribute attribute = maybeAttribute.Value;

        if (await attributeRepository.ExistsBySlugAsync(
                slugResult.Value, attribute.Id, cancellationToken))
            return DomainErrors.ProductAttributes.SlugAlreadyExists();

        attribute.ChangeSlug(slugResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}