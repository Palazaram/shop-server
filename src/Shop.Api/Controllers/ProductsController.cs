using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Abstractions;
using Shop.Application.Products;
using Shop.Application.Products.CreateProduct;
using Shop.Application.Products.SetProductAttributeValues;
using Shop.Application.Products.UpdateProduct;
using Shop.Application.ProductVariants;
using Shop.Application.ProductVariants.CreateProductVariant;
using Shop.Domain.Errors;
using Shop.Domain.Roles;

namespace Shop.Api.Controllers;

[Route("api/products")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class ProductsController(
    ICommandHandler<CreateProductCommand, CreateProductResponse> createProductHandler,
    ICommandHandler<UpdateProductCommand> updateProductHandler,
    ICommandHandler<CreateProductVariantCommand, CreateProductVariantResponse> createVariantHandler,
    ICommandHandler<SetProductAttributeValuesCommand> setAttributeValuesHandler,
    IProductQueries productQueries,
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
    public async Task<IActionResult> GetVariants(Guid productId, CancellationToken cancellationToken)
    {
        IReadOnlyList<ProductVariantListItemResponse> variants =
            await variantQueries.GetByProductAsync(productId, cancellationToken);

        return Ok(variants);
    }

    [HttpPut("{productId:guid}/attribute-values")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetAttributeValues(Guid productId, SetProductAttributeValuesRequest request, CancellationToken cancellationToken)
    {
        SetProductAttributeValuesCommand command = new(productId, request.ValueIds);

        UnitResult<Error> result =
            await setAttributeValuesHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpGet("{productId:guid}/attribute-values")]
    [ProducesResponseType<IReadOnlyList<ProductAttributeValueGroupResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAttributeValues(Guid productId, CancellationToken cancellationToken)
    {
        Maybe<IReadOnlyList<ProductAttributeValueGroupResponse>> result =
            await productQueries.GetAttributeValuesAsync(productId, cancellationToken);

        return result.HasNoValue
            ? ToActionResult(DomainErrors.Products.NotFound())
            : Ok(result.Value);
    }
}