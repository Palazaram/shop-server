using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Errors;
using Shop.Domain.ProductVariants;
using Shop.Domain.SlugHistory;

namespace Shop.Application.ProductVariants.ChangeProductVariantSlug;

internal sealed class ChangeProductVariantSlugCommandHandler(
    IProductVariantRepository variantRepository,
    ISlugHistoryRepository slugHistoryRepository,
    TimeProvider timeProvider,
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

        if (variant.Slug == slugResult.Value)
            return UnitResult.Success<Error>();

        Slug previousSlug = variant.Slug;

        // Живая запись сильнее истории: адрес, который сейчас занимают, перестаёт быть
        // перенаправлением. Иначе один слаг вёл бы и к новому владельцу, и к старому.
        Maybe<SlugHistoryEntry> occupied = await slugHistoryRepository.FindAsync(
            SlugOwnerType.ProductVariant, slugResult.Value, cancellationToken);

        if (occupied.HasValue)
            slugHistoryRepository.Remove(occupied.Value);

        slugHistoryRepository.Add(SlugHistoryEntry.Create(
            SlugOwnerType.ProductVariant, variant.Id, previousSlug, timeProvider.GetUtcNow()));

        variant.ChangeSlug(slugResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}