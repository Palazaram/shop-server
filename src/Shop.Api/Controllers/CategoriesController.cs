using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Abstractions;
using Shop.Application.Categories;
using Shop.Application.Categories.CreateCategory;
using Shop.Application.Categories.RenameCategory;
using Shop.Domain.Errors;
using Shop.Domain.Roles;

namespace Shop.Api.Controllers;

[Route("api/categories")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class CategoriesController(
    ICommandHandler<CreateCategoryCommand, CreateCategoryResponse> createCategoryHandler,
    ICommandHandler<RenameCategoryCommand> renameCategoryHandler,
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
}