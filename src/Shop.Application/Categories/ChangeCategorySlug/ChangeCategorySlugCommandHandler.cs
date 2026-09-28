using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Categories;
using Shop.Domain.Common;
using Shop.Domain.Errors;
using Shop.Domain.SlugHistory;

namespace Shop.Application.Categories.ChangeCategorySlug;

internal sealed class ChangeCategorySlugCommandHandler(
    ICategoryRepository categoryRepository,
    ISlugHistoryRepository slugHistoryRepository,
    TimeProvider timeProvider,
    IUnitOfWork unitOfWork)
        : ICommandHandler<ChangeCategorySlugCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        ChangeCategorySlugCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<Category> maybeCategory =
            await categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);

        if (maybeCategory.HasNoValue)
            return DomainErrors.Categories.NotFound();

        Result<Slug, Error> slugResult = Slug.Create(command.Slug);
        if (slugResult.IsFailure)
            return slugResult.Error;

        Category category = maybeCategory.Value;

        // Исключаем саму категорию: повтор её же слага — не конфликт, а пустая операция.
        if (await categoryRepository.ExistsBySlugAsync(
                slugResult.Value, category.Id, cancellationToken))
            return DomainErrors.Categories.SlugAlreadyExists();

        if (category.Slug == slugResult.Value)
            return UnitResult.Success<Error>();

        Slug previousSlug = category.Slug;

        // Живая запись сильнее истории: адрес, который сейчас занимают, перестаёт быть
        // перенаправлением. Иначе один слаг вёл бы и к новому владельцу, и к старому.
        Maybe<SlugHistoryEntry> occupied = await slugHistoryRepository.FindAsync(
            SlugOwnerType.Category, slugResult.Value, cancellationToken);

        if (occupied.HasValue)
            slugHistoryRepository.Remove(occupied.Value);

        // Прошлый адрес переживает переименование: ссылки и выдача поисковика ведут на него
        // месяцами после того, как админ поправил слаг.
        slugHistoryRepository.Add(SlugHistoryEntry.Create(
            SlugOwnerType.Category, category.Id, previousSlug, timeProvider.GetUtcNow()));

        category.ChangeSlug(slugResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}