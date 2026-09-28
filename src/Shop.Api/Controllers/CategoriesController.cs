using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Api.Extensions;
using Shop.Application.Abstractions;
using Shop.Application.Categories;
using Shop.Application.Categories.ChangeCategorySlug;
using Shop.Application.Categories.CreateCategory;
using Shop.Application.Categories.RenameCategory;
using Shop.Application.Categories.ReorderCategories;
using Shop.Application.Categories.SetCategoryAttributes;
using Shop.Application.Products;
using Shop.Domain.Errors;
using Shop.Domain.Roles;

namespace Shop.Api.Controllers;

[Route("api/categories")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class CategoriesController(
    ICommandHandler<CreateCategoryCommand, CreateCategoryResponse> createCategoryHandler,
    ICommandHandler<RenameCategoryCommand> renameCategoryHandler,
    ICommandHandler<ReorderCategoriesCommand> reorderCategoriesHandler,
    ICommandHandler<SetCategoryAttributesCommand> setAttributesHandler,
    ICommandHandler<ChangeCategorySlugCommand> changeCategorySlugHandler,
    ICategoryQueries categoryQueries,
    IProductListQueries productListQueries,
    IProductFilterQueries productFilterQueries)
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

    [HttpPut("{categoryId:guid}/slug")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangeSlug(
        Guid categoryId,
        ChangeCategorySlugRequest request,
        CancellationToken cancellationToken)
    {
        ChangeCategorySlugCommand command = new(categoryId, request.Slug);

        UnitResult<Error> result =
            await changeCategorySlugHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpPut("order")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reorder(
        ReorderCategoriesCommand command,
        CancellationToken cancellationToken)
    {
        UnitResult<Error> result =
            await reorderCategoriesHandler.HandleAsync(command, cancellationToken);

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
    [ProducesResponseType<ProductFiltersResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFilters(Guid categoryId, CancellationToken cancellationToken)
    {
        Result<ProductFilterSet, Error> filters = ProductListQueryParser.ParseFilters(Request.Query);

        if (filters.IsFailure)
            return ToActionResult(filters.Error);

        Result<ProductFiltersResponse, Error> result =
            await productFilterQueries.GetAsync(categoryId, filters.Value, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ToActionResult(result.Error);
    }

    [HttpGet("{categoryId:guid}/products")]
    [AllowAnonymous]
    [ProducesResponseType<ProductListResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProducts(Guid categoryId, CancellationToken cancellationToken)
    {
        Result<ProductListQuery, Error> query = ProductListQueryParser.Build(categoryId, Request.Query);

        if (query.IsFailure)
            return ToActionResult(query.Error);

        Result<ProductListResponse, Error> result =
            await productListQueries.ListAsync(query.Value, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : ToActionResult(result.Error);
    }
}