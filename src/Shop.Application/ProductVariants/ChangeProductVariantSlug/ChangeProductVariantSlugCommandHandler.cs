using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Errors;
using Shop.Domain.ProductVariants;

namespace Shop.Application.ProductVariants.ChangeProductVariantSlug;

internal sealed class ChangeProductVariantSlugCommandHandler(
    IProductVariantRepository variantRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<ChangeProductVariantSlugCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        ChangeProductVariantSlugCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<ProductVariant> maybeVariant =
            await variantRepository.GetByIdAsync(command.VariantId, cancellationToken);

        if (maybeVariant.HasNoValue)
            return DomainErrors.ProductVariants.NotFound();

        Result<Slug, Error> slugResult = Slug.Create(command.Slug);
        if (slugResult.IsFailure)
            return slugResult.Error;

        ProductVariant variant = maybeVariant.Value;

        if (await variantRepository.ExistsBySlugAsync(
                slugResult.Value, variant.Id, cancellationToken))
            return DomainErrors.ProductVariants.SlugAlreadyExists();

        variant.ChangeSlug(slugResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}