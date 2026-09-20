using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Categories;
using Shop.Application.Products;
using Shop.Domain.Categories;
using Shop.Domain.Errors;
using Shop.Domain.Products;

namespace Shop.Persistence.Queries;

internal sealed class CategoryQueries(AppDbContext context) : ICategoryQueries
{
    public async Task<IReadOnlyList<CategoryTreeItemResponse>> GetTreeAsync(CancellationToken cancellationToken)
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

    public async Task<Result<CategoryFiltersResponse, Error>> GetFiltersAsync(
        Guid categoryId,
        IReadOnlyList<ProductListFilter> filters,
        CancellationToken cancellationToken)
    {
        // Поддерево: категория и её прямые дети. Опирается на ограничение глубины двумя уровнями.
        var nodes = await context.Categories
            .AsNoTracking()
            .Where(c => c.Id == categoryId || c.ParentId == categoryId)
            .Select(c => new { c.Id, c.ParentId })
            .ToListAsync(cancellationToken);

        var self = nodes.Find(n => n.Id == categoryId);

        if (self is null)
            return DomainErrors.Categories.NotFound();

        List<Guid> subtreeIds = [.. nodes.Select(n => n.Id)];

        // Кривой фильтр — ошибка запроса, она не зависит от набора атрибутов категории,
        // поэтому проверяется до любых ранних выходов.
        Result<List<ResolvedAttributeFilter>, Error> resolved =
            await AttributeFilterResolver.ResolveAsync(context, filters, cancellationToken);

        if (resolved.IsFailure)
            return resolved.Error;

        List<ResolvedAttributeFilter> applied = resolved.Value;

        List<Guid> owners = self.ParentId is null
            ? [categoryId]
            : [categoryId, self.ParentId.Value];

        var categoryLinks = await context.Set<CategoryAttribute>()
            .AsNoTracking()
            .Where(ca => owners.Contains(ca.CategoryId))
            .Select(ca => new { ca.CategoryId, ca.AttributeId, ca.DisplayOrder })
            .ToListAsync(cancellationToken);

        bool hasOwn = categoryLinks.Exists(l => l.CategoryId == categoryId);

        Guid owner = CategoryAttributeInheritance.ResolveOwner(self.Id, self.ParentId, hasOwn);

        var effective = categoryLinks
            .Where(l => l.CategoryId == owner)
            .OrderBy(l => l.DisplayOrder)
            .ToList();

        if (effective.Count == 0)
            return new CategoryFiltersResponse([]);

        HashSet<Guid> attributeIds = [.. effective.Select(l => l.AttributeId)];

        IQueryable<ProductAttributeValue> assignments = context.Set<ProductAttributeValue>();

        IQueryable<Guid> MatchingProductIds(IEnumerable<ResolvedAttributeFilter> groups)
        {
            IQueryable<Product> products = context.Products
                .AsNoTracking()
                .Where(p => subtreeIds.Contains(p.CategoryId));

            foreach (ResolvedAttributeFilter group in groups)
            {
                List<Guid> valueIds = group.ValueIds;

                products = products.Where(p => assignments.Any(
                    a => a.ProductId == p.Id && valueIds.Contains(a.AttributeValueId)));
            }

            return products.Select(p => p.Id);
        }

        async Task<Dictionary<Guid, int>> CountByValueAsync(IQueryable<Guid> productIds)
        {
            var counted = await (
                from assignment in assignments
                join variant in context.ProductVariants
                    on assignment.ProductId equals variant.ProductId
                where productIds.Contains(assignment.ProductId)
                group variant by assignment.AttributeValueId into grouped
                select new { ValueId = grouped.Key, Count = grouped.Count() })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return counted.ToDictionary(c => c.ValueId, c => c.Count);
        }

        // Группам без своего выбора хватает одного общего подсчёта по всем фильтрам.
        Dictionary<Guid, int> baseCounts = await CountByValueAsync(MatchingProductIds(applied));

        // Группе со своим выбором нужен подсчёт без её собственного фильтра.
        Dictionary<Guid, Dictionary<Guid, int>> countsByAttribute = [];

        foreach (ResolvedAttributeFilter group in applied)
        {
            countsByAttribute[group.AttributeId] = await CountByValueAsync(
                MatchingProductIds(applied.Where(other => other.AttributeId != group.AttributeId)));
        }

        Dictionary<Guid, HashSet<Guid>> selectedByAttribute =
            applied.ToDictionary(f => f.AttributeId, f => new HashSet<Guid>(f.ValueIds));

        var values = await (
            from value in context.AttributeValues
            join attribute in context.ProductAttributes on value.AttributeId equals attribute.Id
            where attributeIds.Contains(value.AttributeId)
            select new
            {
                value.Id,
                value.AttributeId,
                value.Name,
                value.Slug,
                AttributeName = attribute.Name,
                AttributeSlug = attribute.Slug
            })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var byAttribute = values
            .GroupBy(v => v.AttributeId)
            .ToDictionary(g => g.Key, g => g.ToList());

        List<CategoryFilterGroupResponse> groups = [];

        foreach (var link in effective)
        {
            if (!byAttribute.TryGetValue(link.AttributeId, out var groupValues))
                continue;

            Dictionary<Guid, int> counts =
                countsByAttribute.TryGetValue(link.AttributeId, out var ownCounts)
                    ? ownCounts
                    : baseCounts;

            HashSet<Guid> selected =
                selectedByAttribute.TryGetValue(link.AttributeId, out var ownSelection)
                    ? ownSelection
                    : [];

            var visible = groupValues
                .Select(value => new { Value = value, Count = counts.GetValueOrDefault(value.Id) })
                .Where(entry => entry.Count > 0 || selected.Contains(entry.Value.Id))
                .OrderBy(entry => entry.Value.Name, TextComparers.Ukrainian)
                .ToList();

            if (visible.Count == 0)
                continue;

            var head = visible[0].Value;

            groups.Add(new CategoryFilterGroupResponse(
                link.AttributeId,
                head.AttributeName,
                head.AttributeSlug.Value,
                [.. visible.Select(entry => new CategoryFilterValueResponse(
                    entry.Value.Id,
                    entry.Value.Name,
                    entry.Value.Slug.Value,
                    entry.Count))]));
        }

        return new CategoryFiltersResponse(groups);
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