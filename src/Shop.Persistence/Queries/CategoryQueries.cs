using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Categories;
using Shop.Domain.Categories;
using Shop.Domain.Common;
using Shop.Domain.Errors;

namespace Shop.Persistence.Queries;

internal sealed class CategoryQueries(AppDbContext context) : ICategoryQueries
{
    public async Task<IReadOnlyList<CategoryTreeItemResponse>> GetTreeAsync(CancellationToken cancellationToken)
    {
        var rows = await context.Categories
            .AsNoTracking()
            .Select(c => new { c.Id, c.Name, c.Slug, c.ParentId, c.DisplayOrder })
            .ToListAsync(cancellationToken);

        Dictionary<Guid, List<CategoryTreeItemResponse>> subcategories = rows
            .Where(row => row.ParentId.HasValue)
            .GroupBy(row => row.ParentId!.Value)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderBy(row => row.DisplayOrder)
                    .ThenBy(row => row.Name, TextComparers.Ukrainian)
                    .Select(row => new CategoryTreeItemResponse(
                        row.Id, row.Name, row.Slug.Value, []))
                    .ToList());

        return rows
            .Where(row => row.ParentId is null)
            .OrderBy(row => row.DisplayOrder)
            .ThenBy(row => row.Name, TextComparers.Ukrainian)
            .Select(row => new CategoryTreeItemResponse(
                row.Id,
                row.Name,
                row.Slug.Value,
                subcategories.TryGetValue(row.Id, out List<CategoryTreeItemResponse>? children)
                    ? children
                    : []))
            .ToList();
    }

    public async Task<Maybe<CategoryHeaderResponse>> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken)
    {
        Result<Slug, Error> slugResult = Slug.Create(slug);

        // Неразбираемый слаг — это не «ошибка формата», а просто адрес, которого нет.
        // Slug.Create обрезает пробелы, поэтому «herbitsydy » разобралось бы в ту же запись
        // и дало второй адрес с тем же содержимым. Адрес обязан совпадать с хранимым
        // посимвольно: один ресурс — один URL.
        if (slugResult.IsFailure
            || !string.Equals(slug, slugResult.Value.Value, StringComparison.Ordinal))
            return Maybe<CategoryHeaderResponse>.None;

        Slug parsedSlug = slugResult.Value;

        var row = await context.Categories
            .AsNoTracking()
            .Where(c => c.Slug == parsedSlug)
            .Select(c => new
            {
                c.Id,
                c.Name,
                Slug = c.Slug.Value,
                c.ParentId,
                c.MetaTitle,
                c.MetaDescription
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
            return Maybe<CategoryHeaderResponse>.None;

        var parent = row.ParentId is null
            ? null
            : await context.Categories
                .AsNoTracking()
                .Where(c => c.Id == row.ParentId)
                .Select(c => new { c.Name, Slug = c.Slug.Value })
                .FirstOrDefaultAsync(cancellationToken);

        return new CategoryHeaderResponse(
            row.Id,
            row.Name,
            row.Slug,
            row.ParentId,
            parent?.Name,
            parent?.Slug,
            row.MetaTitle,
            row.MetaDescription);
    }

    public async Task<Maybe<CategoryAttributesResponse>> GetAttributesAsync(Guid categoryId, CancellationToken cancellationToken)
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

        Guid owner = CategoryAttributeInheritance.ResolveOwner(
            category.Id, category.ParentId, own.Count > 0);

        if (owner == category.Id)
            return new CategoryAttributesResponse(false, null, own);

        IReadOnlyList<CategoryAttributeItemResponse> inherited =
            await LoadAttributesAsync(owner, cancellationToken);

        return new CategoryAttributesResponse(true, owner, inherited);
    }

    private async Task<IReadOnlyList<CategoryAttributeItemResponse>> LoadAttributesAsync(Guid categoryId, CancellationToken cancellationToken)
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