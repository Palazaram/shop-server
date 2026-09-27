using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;
using Shop.Domain.Products;

namespace Shop.Application.Products.ReorderProductImages;

internal sealed class ReorderProductImagesCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<ReorderProductImagesCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        ReorderProductImagesCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<Product> maybeProduct =
            await productRepository.GetByIdWithImagesAsync(command.ProductId, cancellationToken);

        if (maybeProduct.HasNoValue)
            return DomainErrors.Products.NotFound();

        UnitResult<Error> reorder = maybeProduct.Value.ReorderImages(command.ImageIds!);
        if (reorder.IsFailure)
            return reorder.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}