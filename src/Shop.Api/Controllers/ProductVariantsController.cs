using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Abstractions;
using Shop.Application.ProductVariants;
using Shop.Application.ProductVariants.UpdateProductVariant;
using Shop.Domain.Errors;
using Shop.Domain.Roles;

namespace Shop.Api.Controllers;

[Route("api/product-variants")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class ProductVariantsController(
    ICommandHandler<UpdateProductVariantCommand> updateVariantHandler,
    IProductVariantQueries variantQueries)
    : ApiControllerBase
{
    [HttpPut("{variantId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid variantId,
        UpdateProductVariantRequest request,
        CancellationToken cancellationToken)
    {
        UpdateProductVariantCommand command = new(
            variantId,
            request.Sku,
            request.Price,
            request.StockQuantity);

        UnitResult<Error> result =
            await updateVariantHandler.HandleAsync(command, cancellationToken);

        return HandleResult(result);
    }

    [HttpGet("{slug}")]
    [AllowAnonymous]
    [ProducesResponseType<ProductVariantDetailResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        Maybe<ProductVariantDetailResponse> variant =
            await variantQueries.GetBySlugAsync(slug, cancellationToken);

        return variant.HasNoValue
            ? ToActionResult(DomainErrors.ProductVariants.NotFound())
            : Ok(variant.Value);
    }
}