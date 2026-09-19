using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Abstractions;
using Shop.Application.AttributeValues.CreateAttributeValue;
using Shop.Application.ProductAttributes;
using Shop.Application.ProductAttributes.CreateProductAttribute;
using Shop.Application.ProductAttributes.RenameProductAttribute;
using Shop.Domain.Errors;
using Shop.Domain.Roles;

namespace Shop.Api.Controllers;

[Route("api/product-attributes")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class ProductAttributesController(
    ICommandHandler<CreateProductAttributeCommand, CreateProductAttributeResponse> createHandler,
    ICommandHandler<RenameProductAttributeCommand> renameHandler,
    ICommandHandler<CreateAttributeValueCommand, CreateAttributeValueResponse> createValueHandler,
    IProductAttributeQueries attributeQueries)
    : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateProductAttributeResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        CreateProductAttributeCommand command,
        CancellationToken cancellationToken)
    {
        Result<CreateProductAttributeResponse, Error> result =
            await createHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result, response =>
            StatusCode(StatusCodes.Status201Created, response));
    }

    [HttpPut("{attributeId:guid}/name")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Rename(
        Guid attributeId,
        RenameProductAttributeRequest request,
        CancellationToken cancellationToken)
    {
        RenameProductAttributeCommand command = new(attributeId, request.Name);

        UnitResult<Error> result = await renameHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<ProductAttributeResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        => Ok(await attributeQueries.GetAllAsync(cancellationToken));

    [HttpPost("{attributeId:guid}/values")]
    [ProducesResponseType<CreateAttributeValueResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateValue(
        Guid attributeId,
        CreateAttributeValueRequest request,
        CancellationToken cancellationToken)
    {
        CreateAttributeValueCommand command = new(attributeId, request.Name, request.Slug);

        Result<CreateAttributeValueResponse, Error> result =
            await createValueHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result, response =>
            StatusCode(StatusCodes.Status201Created, response));
    }

    [HttpGet("{attributeId:guid}/values")]
    [ProducesResponseType<IReadOnlyList<AttributeValueResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetValues(Guid attributeId, CancellationToken cancellationToken)
        => Ok(await attributeQueries.GetValuesAsync(attributeId, cancellationToken));
}