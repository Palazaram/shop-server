using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.AttributeValues;
using Shop.Domain.Categories;
using Shop.Domain.Errors;
using Shop.Domain.Products;

namespace Shop.Application.Products.SetProductAttributeValues;

internal sealed class SetProductAttributeValuesCommandHandler(
    IProductRepository productRepository,
    IAttributeValueRepository valueRepository,
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
    : ICommandHandler<SetProductAttributeValuesCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        SetProductAttributeValuesCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<Product> maybeProduct =
            await productRepository.GetByIdAsync(command.ProductId, cancellationToken);

        if (maybeProduct.HasNoValue)
            return DomainErrors.Products.NotFound();

        Product product = maybeProduct.Value;
        IReadOnlyList<Guid> valueIds = command.ValueIds!;

        IReadOnlyList<AttributeValueRef> refs =
            await valueRepository.GetRefsAsync(valueIds, cancellationToken);

        if (refs.Count != valueIds.Count)
            return DomainErrors.Products.AttributeValueNotFound();

        HashSet<Guid> applicable =
            [.. await categoryRepository.GetEffectiveAttributeIdsAsync(
                product.CategoryId, cancellationToken)];

        List<string> notApplicable = refs
            .Where(reference => !applicable.Contains(reference.AttributeId))
            .Select(reference => reference.ValueName)
            .ToList();

        if (notApplicable.Count > 0)
            return DomainErrors.Products.AttributeNotApplicable(notApplicable);

        UnitResult<Error> result = product.SetAttributeValues(valueIds);
        if (result.IsFailure)
            return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}