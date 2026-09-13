using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Errors;
using Shop.Domain.ProductVariants;

namespace Shop.Application.ProductVariants.UpdateProductVariant;

internal sealed class UpdateProductVariantCommandHandler(
    IProductVariantRepository variantRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<UpdateProductVariantCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        UpdateProductVariantCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<ProductVariant> maybeVariant =
            await variantRepository.GetByIdAsync(command.VariantId, cancellationToken);

        if (maybeVariant.HasNoValue)
            return DomainErrors.ProductVariants.NotFound();

        ProductVariant variant = maybeVariant.Value;

        Result<Money, Error> priceResult = Money.Create(command.Price!.Value);
        if (priceResult.IsFailure)
            return priceResult.Error;

        UnitResult<Error> skuResult = variant.ChangeSku(command.Sku);
        if (skuResult.IsFailure)
            return skuResult;

        UnitResult<Error> stockResult = variant.SetStock(command.StockQuantity!.Value);
        if (stockResult.IsFailure)
            return stockResult;

        variant.ChangePrice(priceResult.Value);

        if (await variantRepository.ExistsBySkuAsync(
                variant.Sku, variant.Id, cancellationToken))
            return DomainErrors.ProductVariants.SkuAlreadyExists();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}