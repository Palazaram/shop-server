using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;
using Shop.Domain.Products;

namespace Shop.Application.Products.SetProductFeatured;

internal sealed class SetProductFeaturedCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<SetProductFeaturedCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        SetProductFeaturedCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<Product> maybeProduct =
            await productRepository.GetByIdAsync(command.ProductId, cancellationToken);

        if (maybeProduct.HasNoValue)
            return DomainErrors.Products.NotFound();

        maybeProduct.Value.SetFeatured(command.IsFeatured!.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}
