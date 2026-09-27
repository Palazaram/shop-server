using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.Application.Abstractions;
using Shop.Application.Products.AddProductImage;
using Shop.Application.Products.ChangeProductImageAlt;
using Shop.Application.Products.RemoveProductImage;
using Shop.Application.Products.ReorderProductImages;
using Shop.Domain.Errors;
using Shop.Domain.Roles;

namespace Shop.Api.Controllers;

[Route("api/products/{productId:guid}/images")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class ProductImagesController(
    ICommandHandler<AddProductImageCommand, AddProductImageResponse> addHandler,
    ICommandHandler<RemoveProductImageCommand> removeHandler,
    ICommandHandler<ReorderProductImagesCommand> reorderHandler,
    ICommandHandler<ChangeProductImageAltCommand> changeAltHandler)
        : ApiControllerBase
{
    private const int MaxFileSizeBytes = 5 * 1024 * 1024;

    // Лимит запроса заведомо выше лимита файла: иначе файл чуть больше пяти мегабайт
    // обрывается Kestrel'ом до контроллера, и клиент получает разрыв соединения
    // вместо понятной ошибки. Запас — на служебные части multipart.
    private const int MaxRequestSizeBytes = 8 * 1024 * 1024;

    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(MaxRequestSizeBytes)]
    [ProducesResponseType<AddProductImageResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Upload(
        Guid productId,
        [FromForm] IFormFile? file,
        [FromForm] string? alt,
        CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0)
            return ToActionResult(DomainErrors.Products.ImageIsRequired());

        if (file.Length > MaxFileSizeBytes)
            return ToActionResult(DomainErrors.Products.ImageFileTooLarge(MaxFileSizeBytes));

        using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, cancellationToken);

        Result<AddProductImageResponse, Error> result = await addHandler.HandleAsync(
            new AddProductImageCommand(productId, buffer.ToArray(), alt),
            cancellationToken);

        return HandleResult(result, response =>
            StatusCode(StatusCodes.Status201Created, response));
    }

    [HttpDelete("{imageId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove(
        Guid productId,
        Guid imageId,
        CancellationToken cancellationToken)
            => HandleResult(await removeHandler.HandleAsync(
                new RemoveProductImageCommand(productId, imageId), cancellationToken));

    [HttpPut("order")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reorder(
        Guid productId,
        ReorderProductImagesRequest request,
        CancellationToken cancellationToken)
            => HandleResult(await reorderHandler.HandleAsync(
                new ReorderProductImagesCommand(productId, request.ImageIds), cancellationToken));

    [HttpPut("{imageId:guid}/alt")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeAlt(
        Guid productId,
        Guid imageId,
        ChangeProductImageAltRequest request,
        CancellationToken cancellationToken)
            => HandleResult(await changeAltHandler.HandleAsync(
                new ChangeProductImageAltCommand(productId, imageId, request.Alt), cancellationToken));
}