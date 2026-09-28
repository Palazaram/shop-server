using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;
using Shop.Domain.Products;

namespace Shop.Application.Products.SetProductSeo;

internal sealed class SetProductSeoCommandHandler(
    IProductRepository productRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<SetProductSeoCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        SetProductSeoCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<Product> maybeProduct =
            await productRepository.GetByIdAsync(command.ProductId, cancellationToken);

        if (maybeProduct.HasNoValue)
            return DomainErrors.Products.NotFound();

        UnitResult<Error> result =
            maybeProduct.Value.SetSeo(command.MetaTitle, command.MetaDescription);

        if (result.IsFailure)
            return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}
