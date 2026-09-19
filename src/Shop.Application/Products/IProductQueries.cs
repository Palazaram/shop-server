using CSharpFunctionalExtensions;
using Shop.Application.ProductAttributes;

namespace Shop.Application.Products;

public sealed record ProductAttributeValueGroupResponse(
    Guid AttributeId,
    string AttributeName,
    string AttributeSlug,
    IReadOnlyList<AttributeValueResponse> Values);

public interface IProductQueries
{
    Task<Maybe<IReadOnlyList<ProductAttributeValueGroupResponse>>> GetAttributeValuesAsync(
        Guid productId,
        CancellationToken cancellationToken);
}