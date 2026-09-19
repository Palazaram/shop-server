using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Abstractions;
using Shop.Application.AttributeValues.RenameAttributeValue;
using Shop.Domain.Errors;
using Shop.Domain.Roles;

namespace Shop.Api.Controllers;

[Route("api/attribute-values")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class AttributeValuesController(
    ICommandHandler<RenameAttributeValueCommand> renameHandler)
    : ApiControllerBase
{
    [HttpPut("{valueId:guid}/name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Rename(
        Guid valueId,
        RenameAttributeValueRequest request,
        CancellationToken cancellationToken)
    {
        RenameAttributeValueCommand command = new(valueId, request.Name);

        UnitResult<Error> result = await renameHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result);
    }
}