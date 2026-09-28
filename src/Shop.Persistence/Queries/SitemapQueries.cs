using Microsoft.EntityFrameworkCore;
using Shop.Application.Catalog;

namespace Shop.Persistence.Queries;

internal sealed class SitemapQueries(AppDbContext context) : ISitemapQueries
{
    public async Task<SitemapResponse> GetAsync(CancellationToken cancellationToken)
    {
        List<string> categorySlugs = await context.Categories
            .AsNoTracking()
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Id)
            .Select(c => c.Slug.Value)
            .ToListAsync(cancellationToken);

        // По одному адресу на препарат — той фасовки, на которую указывает canonical.
        // Порядок «сначала цена, потом id» обязан совпадать с карточкой и подборкой главной:
        // иначе в карте сайта окажется адрес, который сам себя объявляет неканоническим.
        List<string> productSlugs = await (
            from product in context.Products.AsNoTracking()
            let cheapestVariantId = context.ProductVariants
                .Where(v => v.ProductId == product.Id)
                .OrderBy(v => v.Price.Value)
                .ThenBy(v => v.Id)
                .Select(v => (Guid?)v.Id)
                .FirstOrDefault()
            join variant in context.ProductVariants
                on cheapestVariantId equals (Guid?)variant.Id
            orderby product.Id
            select variant.Slug.Value)
            .ToListAsync(cancellationToken);

        return new SitemapResponse(categorySlugs, productSlugs);
    }
}
