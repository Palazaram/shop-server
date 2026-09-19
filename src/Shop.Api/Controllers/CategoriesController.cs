using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Abstractions;
using Shop.Application.Categories;
using Shop.Application.Categories.CreateCategory;
using Shop.Application.Categories.RenameCategory;
using Shop.Application.Categories.SetCategoryAttributes;
using Shop.Domain.Errors;
using Shop.Domain.Roles;

namespace Shop.Api.Controllers;

[Route("api/categories")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class CategoriesController(
    ICommandHandler<CreateCategoryCommand, CreateCategoryResponse> createCategoryHandler,
    ICommandHandler<RenameCategoryCommand> renameCategoryHandler,
    ICommandHandler<SetCategoryAttributesCommand> setAttributesHandler,
    ICategoryQueries categoryQueries)
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
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFilters(Guid categoryId, CancellationToken cancellationToken)
    {
        Maybe<CategoryFiltersResponse> result =
            await categoryQueries.GetFiltersAsync(categoryId, cancellationToken);

        return result.HasNoValue
            ? ToActionResult(DomainErrors.Categories.NotFound())
            : Ok(result.Value);
    }
}