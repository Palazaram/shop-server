using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Domain.Categories;
using Shop.Domain.Common;

namespace Shop.Persistence.Repositories;

internal sealed class CategoryRepository(AppDbContext context) : ICategoryRepository
{
    public async Task<Maybe<Category>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Categories
            .Include(c => c.Attributes)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

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

    public Task<bool> HasChildrenAsync(Guid categoryId, CancellationToken cancellationToken = default)
        => context.Categories.AnyAsync(c => c.ParentId == categoryId, cancellationToken);

    public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        => context.Categories.AnyAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Guid>> GetEffectiveAttributeIdsAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        var category = await context.Categories
            .AsNoTracking()
            .Where(c => c.Id == categoryId)
            .Select(c => new { c.Id, c.ParentId })
            .FirstOrDefaultAsync(cancellationToken);

        if (category is null)
            return [];

        bool hasOwn = await context.Set<CategoryAttribute>()
            .AnyAsync(ca => ca.CategoryId == categoryId, cancellationToken);

        Guid owner = CategoryAttributeInheritance.ResolveOwner(
            category.Id, category.ParentId, hasOwn);

        return await context.Set<CategoryAttribute>()
            .Where(ca => ca.CategoryId == owner)
            .Select(ca => ca.AttributeId)
            .ToListAsync(cancellationToken);
    }

    public void Add(Category category) => context.Categories.Add(category);
}