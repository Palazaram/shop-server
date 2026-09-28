using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Categories;
using Shop.Domain.Common;
using Shop.Domain.Errors;

namespace Shop.Application.Categories.ChangeCategorySlug;

internal sealed class ChangeCategorySlugCommandHandler(
    ICategoryRepository categoryRepository,
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

        category.ChangeSlug(slugResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}