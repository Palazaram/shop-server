using CSharpFunctionalExtensions;
using Shop.Domain.Common;

namespace Shop.Domain.Categories;

public interface ICategoryRepository
{
    Task<Maybe<Category>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(string name, Guid? parentId, Guid? excludeCategoryId, CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(Slug slug, Guid? parentId, CancellationToken cancellationToken = default);

    Task<bool> HasChildrenAsync(Guid categoryId, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Guid>> GetEffectiveAttributeIdsAsync(Guid categoryId, CancellationToken cancellationToken = default);

    void Add(Category category);
}