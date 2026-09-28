using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Api.Extensions;
using Shop.Application.Catalog;
using Shop.Application.Categories;
using Shop.Application.Products;
using Shop.Domain.Errors;

namespace Shop.Api.Controllers;

[Route("api/catalog")]
[AllowAnonymous]
public sealed class CatalogController(
    IProductListQueries productListQueries,
    IProductFilterQueries productFilterQueries,
    ICategoryQueries categoryQueries,
    ISitemapQueries sitemapQueries) : ApiControllerBase
{
    [HttpGet("categories/{slug}")]
    [ProducesResponseType<CategoryHeaderResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(string slug, CancellationToken cancellationToken)
    {
        Maybe<CategoryHeaderResponse> result =
            await categoryQueries.GetBySlugAsync(slug, cancellationToken);

        return result.HasNoValue
            ? ToActionResult(DomainErrors.Categories.NotFound())
            : Ok(result.Value);
    }

    [HttpGet("sitemap")]
    [ProducesResponseType<SitemapResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSitemap(CancellationToken cancellationToken)
        => Ok(await sitemapQueries.GetAsync(cancellationToken));

    [HttpGet("products")]
    [ProducesResponseType<ProductListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetProducts(CancellationToken cancellationToken)
    {
        Result<ProductListQuery, Error> query =
            ProductListQueryParser.Build(categoryId: null, Request.Query);

        if (query.IsFailure)
            return ToActionResult(query.Error);

        Result<ProductListResponse, Error> result =
            await productListQueries.ListAsync(query.Value, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ToActionResult(result.Error);
    }

    [HttpGet("filters")]
    [ProducesResponseType<ProductFiltersResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetFilters(CancellationToken cancellationToken)
    {
        Result<ProductFilterSet, Error> filters =
            ProductListQueryParser.ParseFilters(categoryId: null, Request.Query);

        if (filters.IsFailure)
            return ToActionResult(filters.Error);

        Result<ProductFiltersResponse, Error> result =
            await productFilterQueries.GetAsync(categoryId: null, filters.Value, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ToActionResult(result.Error);
    }

    [HttpGet("featured")]
    [ProducesResponseType<IReadOnlyList<ProductListItemResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFeatured(CancellationToken cancellationToken)
    {
        int limit = ProductListQueryParser.ReadLimit(
            Request.Query, FeaturedProducts.DefaultLimit, FeaturedProducts.MaxLimit);

        IReadOnlyList<ProductListItemResponse> items =
            await productListQueries.GetFeaturedAsync(limit, cancellationToken);

        return Ok(items);
    }
}
