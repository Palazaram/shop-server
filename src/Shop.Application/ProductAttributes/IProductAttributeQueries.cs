namespace Shop.Application.ProductAttributes;

public sealed record ProductAttributeResponse(Guid Id, string Name, string Slug, int ValueCount);

public sealed record AttributeValueResponse(Guid Id, string Name, string Slug);

public interface IProductAttributeQueries
{
    Task<IReadOnlyList<ProductAttributeResponse>> GetAllAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<AttributeValueResponse>> GetValuesAsync(
        Guid attributeId,
        CancellationToken cancellationToken);
}