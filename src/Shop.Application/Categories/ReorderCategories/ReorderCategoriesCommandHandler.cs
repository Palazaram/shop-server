using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Categories;
using Shop.Domain.Errors;

namespace Shop.Application.Categories.ReorderCategories;

internal sealed class ReorderCategoriesCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<ReorderCategoriesCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        ReorderCategoriesCommand command,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Guid> categoryIds = command.CategoryIds!;

        // Родитель приехал в теле, поэтому его отсутствие — 400, а не 404.
        if (command.ParentId.HasValue
            && !await categoryRepository.ExistsAsync(command.ParentId.Value, cancellationToken))
            return DomainErrors.Categories.ParentNotFound();

        IReadOnlyList<Category> siblings =
            await categoryRepository.GetSiblingsAsync(command.ParentId, cancellationToken);

        Dictionary<Guid, Category> byId = siblings.ToDictionary(category => category.Id);

        // Сначала то, что прислал клиент, потом то, что вывел сервер.
        foreach (Guid categoryId in categoryIds)
            if (!byId.ContainsKey(categoryId))
                return DomainErrors.Categories.CategoryNotOnLevel(categoryId);

        if (categoryIds.Count != siblings.Count)
            return DomainErrors.Categories.OrderIsIncomplete(siblings.Count);

        for (int index = 0; index < categoryIds.Count; index++)
            byId[categoryIds[index]].SetDisplayOrder(index);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}