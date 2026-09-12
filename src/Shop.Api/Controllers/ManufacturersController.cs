using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Abstractions;
using Shop.Application.Manufacturers;
using Shop.Application.Manufacturers.CreateManufacturer;
using Shop.Application.Manufacturers.UpdateManufacturer;
using Shop.Domain.Errors;
using Shop.Domain.Roles;

namespace Shop.Api.Controllers;

[Route("api/manufacturers")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class ManufacturersController(
    ICommandHandler<CreateManufacturerCommand, CreateManufacturerResponse> createHandler,
    ICommandHandler<UpdateManufacturerCommand> updateHandler,
    IManufacturerQueries manufacturerQueries)
    : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateManufacturerResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        CreateManufacturerCommand command,
        CancellationToken cancellationToken)
    {
        Result<CreateManufacturerResponse, Error> result =
            await createHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result, response =>
            StatusCode(StatusCodes.Status201Created, response));
    }

    [HttpPut("{manufacturerId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid manufacturerId,
        UpdateManufacturerRequest request,
        CancellationToken cancellationToken)
    {
        UpdateManufacturerCommand command = new(manufacturerId, request.Name, request.Country);

        UnitResult<Error> result = await updateHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType<IReadOnlyList<ManufacturerResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        IReadOnlyList<ManufacturerResponse> manufacturers =
            await manufacturerQueries.GetAllAsync(cancellationToken);

        return Ok(manufacturers);
    }
}