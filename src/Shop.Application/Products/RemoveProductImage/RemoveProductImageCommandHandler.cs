using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;
using Shop.Domain.Products;

namespace Shop.Application.Products.RemoveProductImage;

internal sealed class RemoveProductImageCommandHandler(
    IProductRepository productRepository,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork)
        : ICommandHandler<RemoveProductImageCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        RemoveProductImageCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<Product> maybeProduct =
            await productRepository.GetByIdWithImagesAsync(command.ProductId, cancellationToken);

        if (maybeProduct.HasNoValue)
            return DomainErrors.Products.NotFound();

        UnitResult<Error> removal = maybeProduct.Value.RemoveImage(command.ImageId);
        if (removal.IsFailure)
            return removal.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Здесь порядок обратный: сначала коммит, потом диск. Упадёт удаление файлов —
        // останутся сироты, которых никто не показывает.
        await imageStorage.DeleteAsync(
            ProductImagePaths.AllKeys(command.ProductId, command.ImageId),
            cancellationToken);

        return UnitResult.Success<Error>();
    }
}