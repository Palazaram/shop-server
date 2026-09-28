using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Categories;
using Shop.Domain.Common;
using Shop.Domain.Errors;
using Shop.Domain.SlugHistory;

namespace Shop.Application.Categories.CreateCategory;

internal sealed class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ISlugGenerator slugGenerator,
    ISlugHistoryRepository slugHistoryRepository,
    IUnitOfWork unitOfWork) 
        : ICommandHandler<CreateCategoryCommand, CreateCategoryResponse>
{
    public async Task<Result<CreateCategoryResponse, Error>> HandleAsync(
        CreateCategoryCommand command,
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
                : DomainErrors.Categories.SlugCannotBeGenerated();

        int displayOrder =
            await categoryRepository.GetNextDisplayOrderAsync(command.ParentId, cancellationToken);

        Result<Category, Error> categoryResult =
            Category.Create(command.Name, slugResult.Value, command.ParentId, displayOrder);
        if (categoryResult.IsFailure)
            return categoryResult.Error;

        Category category = categoryResult.Value;

        if (category.ParentId.HasValue)
        {
            Maybe<Category> parent =
                await categoryRepository.GetByIdAsync(category.ParentId.Value, cancellationToken);

            if (parent.HasNoValue)
                return DomainErrors.Categories.ParentNotFound();

            if (parent.Value.ParentId.HasValue)
                return DomainErrors.Categories.MaxDepthExceeded();
        }

        if (await categoryRepository.ExistsByNameAsync(
                category.Name, category.ParentId, null, cancellationToken))
            return DomainErrors.Categories.NameAlreadyExists();

        if (await categoryRepository.ExistsBySlugAsync(
                category.Slug, null, cancellationToken))
            return DomainErrors.Categories.SlugAlreadyExists();

        // Живая запись сильнее истории: адрес, который сейчас занимают, перестаёт быть
        // перенаправлением. Иначе один слаг вёл бы и к новому владельцу, и к старому.
        Maybe<SlugHistoryEntry> occupied = await slugHistoryRepository.FindAsync(
            SlugOwnerType.Category, category.Slug, cancellationToken);

        if (occupied.HasValue)
            slugHistoryRepository.Remove(occupied.Value);

        categoryRepository.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateCategoryResponse(category.Id, category.Slug.Value);
    }
}