using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;
using Shop.Domain.Products;
using Shop.Domain.Specifications;

namespace Shop.Application.Products.SetProductSpecifications;

internal sealed class SetProductSpecificationsCommandHandler(
    IProductRepository productRepository,
    ISpecificationRepository specificationRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<SetProductSpecificationsCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        SetProductSpecificationsCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<Product> maybeProduct = await productRepository.GetByIdWithSpecificationsAsync(
            command.ProductId, cancellationToken);

        if (maybeProduct.HasNoValue)
            return DomainErrors.Products.NotFound();

        IReadOnlyList<ProductSpecificationItem> items = command.Specifications!;

        List<Guid> ids = [.. items.Select(item => item.SpecificationId!.Value)];

        // Существование характеристики — межагрегатная проверка, поэтому она здесь,
        // а не в домене товара.
        if (!await specificationRepository.AllExistAsync(ids, cancellationToken))
            return DomainErrors.Products.SpecificationNotFound();

        UnitResult<Error> result = maybeProduct.Value.SetSpecifications(
            [.. items.Select(item => new SpecificationValue(
                item.SpecificationId!.Value, item.Value))]);

        if (result.IsFailure)
            return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}
