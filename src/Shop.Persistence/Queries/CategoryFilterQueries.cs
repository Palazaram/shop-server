using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Abstractions;
using Shop.Application.Categories;
using Shop.Application.Products;
using Shop.Domain.Categories;
using Shop.Domain.Errors;
using Shop.Domain.Products;

namespace Shop.Persistence.Queries;

internal sealed class CategoryFilterQueries(AppDbContext context, ISlugGenerator slugGenerator)
    : ICategoryFilterQueries
{
    private const string ManufacturerGroupName = "Виробник";
    private const string CountryGroupName = "Країна походження";

    public async Task<Result<CategoryFiltersResponse, Error>> GetAsync(
        Guid categoryId,
        IReadOnlyList<ProductFilter> filters,
        CancellationToken cancellationToken)
    {
        var nodes = await context.Categories
            .AsNoTracking()
            .Where(c => c.Id == categoryId || c.ParentId == categoryId)
            .Select(c => new { c.Id, c.ParentId })
            .ToListAsync(cancellationToken);

        var self = nodes.Find(n => n.Id == categoryId);

        if (self is null)
            return DomainErrors.Categories.NotFound();

        List<Guid> subtreeIds = [.. nodes.Select(n => n.Id)];

        Result<List<ResolvedFilter>, Error> resolved = await ProductFilterResolver
            .ResolveAsync(context, slugGenerator, filters, cancellationToken);

        if (resolved.IsFailure)
            return resolved.Error;

        List<ResolvedFilter> applied = resolved.Value;

        Dictionary<string, HashSet<string>> selected = filters.ToDictionary(
            f => f.GroupKey,
            f => new HashSet<string>(f.ValueKeys, StringComparer.Ordinal),
            StringComparer.Ordinal);

        IQueryable<ProductAttributeValue> assignments = context.Set<ProductAttributeValue>();

        // Счётчики группы считаются со всеми фильтрами, кроме фильтра самой группы.
        IQueryable<Guid> Matching(string? exceptGroupKey)
            => ProductFilterResolver.MatchingProductIds(
                context,
                subtreeIds,
                exceptGroupKey is null
                    ? applied
                    : applied.Where(f => f.GroupKey != exceptGroupKey));

        async Task<Dictionary<Guid, int>> CountByValueAsync(IQueryable<Guid> productIds)
        {
            var counted = await (
                from assignment in assignments
                join variant in context.ProductVariants
                    on assignment.ProductId equals variant.ProductId
                where productIds.Contains(assignment.ProductId)
                group variant by assignment.AttributeValueId into grouped
                select new { Key = grouped.Key, Count = grouped.Count() })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return counted.ToDictionary(c => c.Key, c => c.Count);
        }

        async Task<Dictionary<Guid, int>> CountByManufacturerAsync(IQueryable<Guid> productIds)
        {
            var counted = await (
                from variant in context.ProductVariants
                join product in context.Products on variant.ProductId equals product.Id
                where productIds.Contains(product.Id)
                group variant by product.ManufacturerId into grouped
                select new { Key = grouped.Key, Count = grouped.Count() })
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return counted.ToDictionary(c => c.Key, c => c.Count);
        }

        List<CategoryFilterGroupResponse> groups = [];

        // --- Группы атрибутов

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

        if (effective.Count > 0)
        {
            HashSet<Guid> attributeIds = [.. effective.Select(l => l.AttributeId)];

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

            Dictionary<Guid, int> baseCounts = await CountByValueAsync(Matching(null));

            foreach (var link in effective)
            {
                if (!byAttribute.TryGetValue(link.AttributeId, out var groupValues))
                    continue;

                string groupKey = ProductFilter.AttributePrefix + groupValues[0].AttributeSlug.Value;

                HashSet<string> chosen = selected.GetValueOrDefault(groupKey, []);

                Dictionary<Guid, int> counts = chosen.Count == 0
                    ? baseCounts
                    : await CountByValueAsync(Matching(groupKey));

                var visible = groupValues
                    .Select(value => new
                    {
                        Key = value.Slug.Value,
                        value.Name,
                        Count = counts.GetValueOrDefault(value.Id)
                    })
                    .Where(entry => entry.Count > 0 || chosen.Contains(entry.Key))
                    .OrderBy(entry => entry.Name, TextComparers.Ukrainian)
                    .ToList();

                if (visible.Count == 0)
                    continue;

                groups.Add(new CategoryFilterGroupResponse(
                    groupKey,
                    groupValues[0].AttributeName,
                    [.. visible.Select(entry => new CategoryFilterValueResponse(
                        entry.Key, entry.Name, entry.Count))]));
            }
        }

        // --- Производитель и страна

        List<ManufacturerRef> manufacturers = await ProductFilterResolver
            .LoadManufacturersAsync(context, slugGenerator, cancellationToken);

        HashSet<string> chosenManufacturers =
            selected.GetValueOrDefault(ProductFilter.ManufacturerKey, []);

        HashSet<string> chosenCountries =
            selected.GetValueOrDefault(ProductFilter.CountryKey, []);

        Dictionary<Guid, int> baseManufacturerCounts =
            await CountByManufacturerAsync(Matching(null));

        Dictionary<Guid, int> manufacturerCounts = chosenManufacturers.Count == 0
            ? baseManufacturerCounts
            : await CountByManufacturerAsync(Matching(ProductFilter.ManufacturerKey));

        var visibleManufacturers = manufacturers
            .Select(m => new
            {
                m.Slug,
                m.Name,
                Count = manufacturerCounts.GetValueOrDefault(m.Id)
            })
            .Where(entry => entry.Count > 0 || chosenManufacturers.Contains(entry.Slug))
            .OrderBy(entry => entry.Name, TextComparers.Ukrainian)
            .ToList();

        if (visibleManufacturers.Count > 0)
        {
            groups.Add(new CategoryFilterGroupResponse(
                ProductFilter.ManufacturerKey,
                ManufacturerGroupName,
                [.. visibleManufacturers.Select(entry => new CategoryFilterValueResponse(
                    entry.Slug, entry.Name, entry.Count))]));
        }

        Dictionary<Guid, int> countryCounts = chosenCountries.Count == 0
            ? baseManufacturerCounts
            : await CountByManufacturerAsync(Matching(ProductFilter.CountryKey));

        // Страна не имеет собственного ключа, поэтому группируем по вычисленному,
        // а не по исходной строке: иначе сайдбар и фильтр разойдутся.
        var visibleCountries = manufacturers
            .Where(m => m.CountryKey.Length > 0)
            .GroupBy(m => m.CountryKey, StringComparer.Ordinal)
            .Select(g => new
            {
                Key = g.Key,
                Name = g.Select(m => m.Country).OrderBy(c => c, TextComparers.Ukrainian).First(),
                Count = g.Sum(m => countryCounts.GetValueOrDefault(m.Id))
            })
            .Where(entry => entry.Count > 0 || chosenCountries.Contains(entry.Key))
            .OrderBy(entry => entry.Name, TextComparers.Ukrainian)
            .ToList();

        if (visibleCountries.Count > 0)
        {
            groups.Add(new CategoryFilterGroupResponse(
                ProductFilter.CountryKey,
                CountryGroupName,
                [.. visibleCountries.Select(entry => new CategoryFilterValueResponse(
                    entry.Key, entry.Name, entry.Count))]));
        }

        return new CategoryFiltersResponse(groups);
    }
}