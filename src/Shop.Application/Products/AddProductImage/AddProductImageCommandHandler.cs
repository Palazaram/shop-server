using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;
using Shop.Domain.Products;

namespace Shop.Application.Products.AddProductImage;

internal sealed class AddProductImageCommandHandler(
    IProductRepository productRepository,
    IImageProcessor imageProcessor,
    IImageStorage imageStorage,
    IUnitOfWork unitOfWork)
        : ICommandHandler<AddProductImageCommand, AddProductImageResponse>
{
    public async Task<Result<AddProductImageResponse, Error>> HandleAsync(
        AddProductImageCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<Product> maybeProduct =
            await productRepository.GetByIdWithImagesAsync(command.ProductId, cancellationToken);

        if (maybeProduct.HasNoValue)
            return DomainErrors.Products.NotFound();

        Product product = maybeProduct.Value;

        using var source = new MemoryStream(command.Content);

        Result<IReadOnlyList<ImageVariant>, Error> variants = imageProcessor.CreateVariants(source);
        if (variants.IsFailure)
            return variants.Error;

        Result<ProductImage, Error> image = product.AddImage(command.Alt);
        if (image.IsFailure)
            return image.Error;

        // Файлы пишем до коммита. Упадёт запись — на диске останутся лишние файлы, и это
        // безобидно; упади мы после коммита, в базе была бы картинка без файла, то есть
        // битое изображение на витрине.
        foreach (ImageVariant variant in variants.Value)
            await imageStorage.SaveAsync(
                ProductImagePaths.Key(product.Id, image.Value.Id, variant.Size),
                variant.Content,
                cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new AddProductImageResponse(image.Value.Id);
    }
}