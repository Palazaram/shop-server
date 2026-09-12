using Microsoft.EntityFrameworkCore;
using Shop.Application.Categories;

namespace Shop.Persistence.Queries;

internal sealed class CategoryQueries(AppDbContext context) : ICategoryQueries
{
    public async Task<IReadOnlyList<CategoryTreeItemResponse>> GetTreeAsync(
        CancellationToken cancellationToken)
    {
        var rows = await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new { c.Id, c.Name, c.Slug, c.ParentId })
            .ToListAsync(cancellationToken);

        Dictionary<Guid, List<CategoryTreeItemResponse>> subcategories = rows
            .Where(row => row.ParentId.HasValue)
            .GroupBy(row => row.ParentId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group
                    .Select(row => new CategoryTreeItemResponse(
                        row.Id, row.Name, row.Slug.Value, []))
                    .ToList());

        return rows
            .Where(row => row.ParentId is null)
            .Select(row => new CategoryTreeItemResponse(
                row.Id,
                row.Name,
                row.Slug.Value,
                subcategories.TryGetValue(row.Id, out List<CategoryTreeItemResponse>? children)
                    ? children
                    : []))
            .ToList();
    }
}