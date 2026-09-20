using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Shop.Application.Abstractions;
using Shop.Application.Categories;
using Shop.Application.Categories.CreateCategory;
using Shop.Application.Categories.RenameCategory;
using Shop.Application.Categories.SetCategoryAttributes;
using Shop.Application.Products;
using Shop.Domain.Errors;
using Shop.Domain.Roles;
using System.Globalization;

namespace Shop.Api.Controllers;

[Route("api/categories")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class CategoriesController(
    ICommandHandler<CreateCategoryCommand, CreateCategoryResponse> createCategoryHandler,
    ICommandHandler<RenameCategoryCommand> renameCategoryHandler,
    ICommandHandler<SetCategoryAttributesCommand> setAttributesHandler,
    ICategoryQueries categoryQueries,
    IProductListQueries productListQueries,
    ICategoryFilterQueries categoryFilterQueries)
        : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateCategoryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        Result<CreateCategoryResponse, Error> result =
            await createCategoryHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result, response =>
            StatusCode(StatusCodes.Status201Created, response));
    }

    [HttpPut("{categoryId:guid}/name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Rename(
    Guid categoryId,
    RenameCategoryRequest request,
    CancellationToken cancellationToken)
    {
        RenameCategoryCommand command = new(categoryId, request.Name);

        UnitResult<Error> result =
            await renameCategoryHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<CategoryTreeItemResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTree(CancellationToken cancellationToken)
    {
        IReadOnlyList<CategoryTreeItemResponse> tree =
            await categoryQueries.GetTreeAsync(cancellationToken);

        return Ok(tree);
    }

    [HttpPut("{categoryId:guid}/attributes")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetAttributes(Guid categoryId, SetCategoryAttributesRequest request, CancellationToken cancellationToken)
    {
        SetCategoryAttributesCommand command = new(categoryId, request.AttributeIds);

        UnitResult<Error> result =
            await setAttributesHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpGet("{categoryId:guid}/attributes")]
    [ProducesResponseType<CategoryAttributesResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttributes(Guid categoryId, CancellationToken cancellationToken)
    {
        Maybe<CategoryAttributesResponse> result =
            await categoryQueries.GetAttributesAsync(categoryId, cancellationToken);

        return result.HasNoValue
            ? ToActionResult(DomainErrors.Categories.NotFound())
            : Ok(result.Value);
    }

    [HttpGet("{categoryId:guid}/filters")]
    [AllowAnonymous]
    [ProducesResponseType<CategoryFiltersResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFilters(Guid categoryId, CancellationToken cancellationToken)
    {
        Result<ProductFilterSet, Error> filters = ParseFilters(Request.Query);

        if (filters.IsFailure)
            return ToActionResult(filters.Error);

        Result<CategoryFiltersResponse, Error> result =
            await categoryFilterQueries.GetAsync(categoryId, filters.Value, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ToActionResult(result.Error);
    }

    [HttpGet("{categoryId:guid}/products")]
    [AllowAnonymous]
    [ProducesResponseType<ProductListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProducts(Guid categoryId, CancellationToken cancellationToken)
    {
        Result<ProductListQuery, Error> query = BuildProductListQuery(categoryId, Request.Query);

        if (query.IsFailure)
            return ToActionResult(query.Error);

        Result<ProductListResponse, Error> result =
            await productListQueries.ListAsync(query.Value, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ToActionResult(result.Error);
    }

    private static Result<ProductFilterSet, Error> ParseFilters(IQueryCollection source)
    {
        List<ProductFilter> filters = [];

        foreach (KeyValuePair<string, StringValues> pair in source)
        {
            bool isAttribute =
                pair.Key.StartsWith(ProductFilter.AttributePrefix, StringComparison.Ordinal)
                && pair.Key.Length > ProductFilter.AttributePrefix.Length;

            bool isFixed = pair.Key is ProductFilter.ManufacturerKey
                                   or ProductFilter.CountryKey
                                   or ProductFilter.PackagingKey;

            if (!isAttribute && !isFixed)
                continue;

            string[] valueKeys = [.. pair.Value
            .SelectMany(raw => (raw ?? string.Empty).Split(
                ',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            .Distinct(StringComparer.Ordinal)];

            if (valueKeys.Length == 0)
                continue;

            filters.Add(new ProductFilter(pair.Key, valueKeys));
        }

        (decimal? Value, string? Invalid) min = ReadPrice(source, ProductFilterSet.PriceMinKey);

        if (min.Invalid is not null)
            return DomainErrors.Products.InvalidPrice(ProductFilterSet.PriceMinKey, min.Invalid);

        (decimal? Value, string? Invalid) max = ReadPrice(source, ProductFilterSet.PriceMaxKey);

        if (max.Invalid is not null)
            return DomainErrors.Products.InvalidPrice(ProductFilterSet.PriceMaxKey, max.Invalid);

        return new ProductFilterSet(filters, min.Value, max.Value);
    }

    private static (decimal? Value, string? Invalid) ReadPrice(IQueryCollection source, string key)
    {
        string? raw = source[key];

        if (string.IsNullOrWhiteSpace(raw))
            return (null, null);

        return decimal.TryParse(raw, PriceStyles, CultureInfo.InvariantCulture, out decimal parsed)
            ? (parsed, null)
            : (null, raw);
    }

    private const NumberStyles PriceStyles =
        NumberStyles.AllowLeadingWhite
        | NumberStyles.AllowTrailingWhite
        | NumberStyles.AllowLeadingSign
        | NumberStyles.AllowDecimalPoint;

    private static Result<ProductListQuery, Error> BuildProductListQuery(
        Guid categoryId, IQueryCollection source)
    {
        Result<ProductFilterSet, Error> filters = ParseFilters(source);

        if (filters.IsFailure)
            return filters.Error;

        int page = int.TryParse(source["page"], NumberStyles.Integer, CultureInfo.InvariantCulture,
            out int parsedPage) && parsedPage > 0 ? parsedPage : 1;

        int pageSize = int.TryParse(source["pageSize"], NumberStyles.Integer, CultureInfo.InvariantCulture,
            out int parsedSize)
            ? Math.Clamp(parsedSize, 1, ProductListQuery.MaxPageSize)
            : ProductListQuery.DefaultPageSize;

        return new ProductListQuery(categoryId, filters.Value, source["sort"], page, pageSize);
    }
}