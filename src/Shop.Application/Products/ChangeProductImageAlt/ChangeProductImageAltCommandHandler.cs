using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;
using Shop.Domain.Products;

namespace Shop.Application.Products.ChangeProductImageAlt;

internal sealed class ChangeProductImageAltCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<ChangeProductImageAltCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        ChangeProductImageAltCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<Product> maybeProduct =
            await productRepository.GetByIdWithImagesAsync(command.ProductId, cancellationToken);

        if (maybeProduct.HasNoValue)
            return DomainErrors.Products.NotFound();

        UnitResult<Error> change = maybeProduct.Value.ChangeImageAlt(command.ImageId, command.Alt);
        if (change.IsFailure)
            return change.Error;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}