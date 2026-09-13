using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Errors;
using Shop.Domain.ProductVariants;
using Shop.Domain.Products;

namespace Shop.Application.ProductVariants.CreateProductVariant;

internal sealed class CreateProductVariantCommandHandler(
    IProductVariantRepository variantRepository,
    IProductRepository productRepository,
    ISlugGenerator slugGenerator,
    IUnitOfWork unitOfWork)
        : ICommandHandler<CreateProductVariantCommand, CreateProductVariantResponse>
{
    public async Task<Result<CreateProductVariantResponse, Error>> HandleAsync(
        CreateProductVariantCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<Product> maybeProduct =
            await productRepository.GetByIdAsync(command.ProductId, cancellationToken);

        if (maybeProduct.HasNoValue)
            return DomainErrors.ProductVariants.ProductNotFound();

        Product product = maybeProduct.Value;

        Result<Packaging, Error> packagingResult = Packaging.Create(
            command.PackagingValue!.Value,
            command.PackagingUnit!.Value);

        if (packagingResult.IsFailure)
            return packagingResult.Error;

        Packaging packaging = packagingResult.Value;

        Result<Money, Error> priceResult = Money.Create(command.Price!.Value);
        if (priceResult.IsFailure)
            return priceResult.Error;

        bool slugProvided = !string.IsNullOrWhiteSpace(command.Slug);

        string slugSource = slugProvided
            ? command.Slug!
            : slugGenerator.Generate($"{product.Name} {packaging}");

        Result<Slug, Error> slugResult = Slug.Create(slugSource);
        if (slugResult.IsFailure)
            return slugProvided
                ? slugResult.Error
                : DomainErrors.ProductVariants.SlugCannotBeGenerated();

        Result<ProductVariant, Error> variantResult = ProductVariant.Create(
            productId: product.Id,
            sku: command.Sku,
            slug: slugResult.Value,
            packaging: packaging,
            price: priceResult.Value,
            stockQuantity: command.StockQuantity!.Value);

        if (variantResult.IsFailure)
            return variantResult.Error;

        ProductVariant variant = variantResult.Value;

        if (await variantRepository.ExistsBySkuAsync(variant.Sku, null, cancellationToken))
            return DomainErrors.ProductVariants.SkuAlreadyExists();

        if (await variantRepository.ExistsByPackagingAsync(
                variant.ProductId, variant.Packaging, cancellationToken))
            return DomainErrors.ProductVariants.PackagingAlreadyExists();

        if (await variantRepository.ExistsBySlugAsync(variant.Slug, cancellationToken))
            return slugProvided
                ? DomainErrors.ProductVariants.SlugAlreadyExists()
                : DomainErrors.ProductVariants.GeneratedSlugAlreadyExists();

        variantRepository.Add(variant);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateProductVariantResponse(variant.Id, variant.Slug.Value);
    }
}