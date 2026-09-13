using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Abstractions;
using Shop.Application.ProductVariants;
using Shop.Application.ProductVariants.CreateProductVariant;
using Shop.Application.Products.CreateProduct;
using Shop.Application.Products.UpdateProduct;
using Shop.Domain.Errors;
using Shop.Domain.Roles;

namespace Shop.Api.Controllers;

[Route("api/products")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class ProductsController(
    ICommandHandler<CreateProductCommand, CreateProductResponse> createProductHandler,
    ICommandHandler<UpdateProductCommand> updateProductHandler,
    ICommandHandler<CreateProductVariantCommand, CreateProductVariantResponse> createVariantHandler,
    IProductVariantQueries variantQueries)
    : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateProductResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        Result<CreateProductResponse, Error> result =
            await createProductHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result, response =>
            StatusCode(StatusCodes.Status201Created, response));
    }

    [HttpPut("{productId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid productId,
        UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        UpdateProductCommand command = new(
            productId,
            request.Name,
            request.Description,
            request.CategoryId,
            request.ManufacturerId);

        UnitResult<Error> result = await updateProductHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpPost("{productId:guid}/variants")]
    [ProducesResponseType<CreateProductVariantResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateVariant(
        Guid productId,
        CreateProductVariantRequest request,
        CancellationToken cancellationToken)
    {
        CreateProductVariantCommand command = new(
            productId,
            request.Sku,
            request.PackagingValue,
            request.PackagingUnit,
            request.Price,
            request.StockQuantity,
            request.Slug);

        Result<CreateProductVariantResponse, Error> result =
            await createVariantHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result, response =>
            StatusCode(StatusCodes.Status201Created, response));
    }

    [HttpGet("{productId:guid}/variants")]
    [ProducesResponseType<IReadOnlyList<ProductVariantListItemResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVariants(
        Guid productId,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<ProductVariantListItemResponse> variants =
            await variantQueries.GetByProductAsync(productId, cancellationToken);

        return Ok(variants);
    }
}