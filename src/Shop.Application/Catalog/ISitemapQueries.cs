namespace Shop.Application.Catalog;

/// <summary>
/// Сырьё для карты сайта. Сам `sitemap.xml` собирает фронт: адреса в нём — фронтовые
/// (`/catalog/herbitsydy`), и бэк про их вид ничего не знает.
/// </summary>
public sealed record SitemapResponse(
    IReadOnlyList<string> CategorySlugs,
    IReadOnlyList<string> ProductSlugs);

public interface ISitemapQueries
{
    Task<SitemapResponse> GetAsync(CancellationToken cancellationToken);
}
