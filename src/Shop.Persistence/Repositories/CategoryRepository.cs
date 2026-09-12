using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Domain.Categories;
using Shop.Domain.Common;

namespace Shop.Persistence.Repositories;

internal sealed class CategoryRepository(AppDbContext context) : ICategoryRepository
{
    public async Task<Maybe<Category>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Categories.FindAsync([id], cancellationToken);

    public Task<bool> ExistsByNameAsync(string name, Guid? parentId, Guid? excludeCategoryId, CancellationToken cancellationToken = default)
    {
        IQueryable<Category> query = context.Categories
            .Where(c => c.Name == name && c.ParentId == parentId);

        if (excludeCategoryId.HasValue)
            query = query.Where(c => c.Id != excludeCategoryId.Value);

        return query.AnyAsync(cancellationToken);
    }

    public Task<bool> ExistsBySlugAsync(Slug slug, Guid? parentId, CancellationToken cancellationToken = default)
        => context.Categories.AnyAsync(c => c.Slug == slug && c.ParentId == parentId, cancellationToken);

    public void Add(Category category) => context.Categories.Add(category);
}