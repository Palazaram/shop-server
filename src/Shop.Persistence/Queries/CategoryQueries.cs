using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Categories;
using Shop.Domain.Categories;

namespace Shop.Persistence.Queries;

internal sealed class CategoryQueries(AppDbContext context) : ICategoryQueries
{
    public async Task<IReadOnlyList<CategoryTreeItemResponse>> GetTreeAsync(
        CancellationToken cancellationToken)
    {
        var rows = await context.Categories
            .AsNoTracking()
            .Select(c => new { c.Id, c.Name, c.Slug, c.ParentId })
            .ToListAsync(cancellationToken);

        Dictionary<Guid, List<CategoryTreeItemResponse>> subcategories = rows
            .Where(row => row.ParentId.HasValue)
            .GroupBy(row => row.ParentId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(row => row.Name, TextComparers.Ukrainian)
                    .Select(row => new CategoryTreeItemResponse(
                        row.Id, row.Name, row.Slug.Value, []))
                    .ToList());

        return rows
            .Where(row => row.ParentId is null)
            .OrderBy(row => row.Name, TextComparers.Ukrainian)
            .Select(row => new CategoryTreeItemResponse(
                row.Id,
                row.Name,
                row.Slug.Value,
                subcategories.TryGetValue(row.Id, out List<CategoryTreeItemResponse>? children)
                    ? children
                    : []))
            .ToList();
    }

    public async Task<Maybe<CategoryAttributesResponse>> GetAttributesAsync(
    Guid categoryId,
    CancellationToken cancellationToken)
    {
        var category = await context.Categories
            .AsNoTracking()
            .Where(c => c.Id == categoryId)
            .Select(c => new { c.Id, c.ParentId })
            .FirstOrDefaultAsync(cancellationToken);

        if (category is null)
            return Maybe<CategoryAttributesResponse>.None;

        IReadOnlyList<CategoryAttributeItemResponse> own =
            await LoadAttributesAsync(category.Id, cancellationToken);

        if (own.Count > 0 || category.ParentId is null)
            return new CategoryAttributesResponse(false, null, own);

        IReadOnlyList<CategoryAttributeItemResponse> inherited =
            await LoadAttributesAsync(category.ParentId.Value, cancellationToken);

        return new CategoryAttributesResponse(true, category.ParentId, inherited);
    }

    private async Task<IReadOnlyList<CategoryAttributeItemResponse>> LoadAttributesAsync(
        Guid categoryId,
        CancellationToken cancellationToken)
    {
        var rows = await (
            from link in context.Set<CategoryAttribute>().AsNoTracking()
            join attribute in context.ProductAttributes on link.AttributeId equals attribute.Id
            where link.CategoryId == categoryId
            orderby link.DisplayOrder
            select new { attribute.Id, attribute.Name, attribute.Slug })
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new CategoryAttributeItemResponse(row.Id, row.Name, row.Slug.Value))
            .ToList();
    }
}