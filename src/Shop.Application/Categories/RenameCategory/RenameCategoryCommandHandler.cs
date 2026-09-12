using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Categories;
using Shop.Domain.Errors;

namespace Shop.Application.Categories.RenameCategory;

internal sealed class RenameCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<RenameCategoryCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        RenameCategoryCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<Category> maybeCategory =
            await categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);

        if (maybeCategory.HasNoValue)
            return DomainErrors.Categories.NotFound();

        Category category = maybeCategory.Value;

        UnitResult<Error> renameResult = category.Rename(command.Name);
        if (renameResult.IsFailure)
            return renameResult;

        if (await categoryRepository.ExistsByNameAsync(
                category.Name, category.ParentId, category.Id, cancellationToken))
            return DomainErrors.Categories.NameAlreadyExists();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}