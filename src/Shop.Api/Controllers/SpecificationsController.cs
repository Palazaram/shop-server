using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Abstractions;
using Shop.Application.Specifications;
using Shop.Application.Specifications.CreateSpecification;
using Shop.Application.Specifications.RenameSpecification;
using Shop.Application.Specifications.ReorderSpecifications;
using Shop.Domain.Errors;
using Shop.Domain.Roles;

namespace Shop.Api.Controllers;

[Route("api/specifications")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class SpecificationsController(
    ICommandHandler<CreateSpecificationCommand, CreateSpecificationResponse> createHandler,
    ICommandHandler<RenameSpecificationCommand> renameHandler,
    ICommandHandler<ReorderSpecificationsCommand> reorderHandler,
    ISpecificationQueries specificationQueries)
        : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateSpecificationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        CreateSpecificationCommand command,
        CancellationToken cancellationToken)
    {
        Result<CreateSpecificationResponse, Error> result =
            await createHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result, response =>
            StatusCode(StatusCodes.Status201Created, response));
    }

    [HttpPut("{specificationId:guid}/name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Rename(
        Guid specificationId,
        RenameSpecificationRequest request,
        CancellationToken cancellationToken)
    {
        RenameSpecificationCommand command = new(specificationId, request.Name);

        UnitResult<Error> result = await renameHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpPut("order")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reorder(
        ReorderSpecificationsCommand command,
        CancellationToken cancellationToken)
    {
        UnitResult<Error> result = await reorderHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<SpecificationResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await specificationQueries.GetAllAsync(cancellationToken));
}
