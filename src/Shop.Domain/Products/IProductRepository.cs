using CSharpFunctionalExtensions;

namespace Shop.Domain.Products;

public interface IProductRepository
{
    Task<Maybe<Product>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<Maybe<Product>> GetByIdWithImagesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Maybe<Product>> GetByIdWithSpecificationsAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> ExistsByNameAsync(
        string name,
        Guid manufacturerId,
        Guid? excludeProductId,
        CancellationToken cancellationToken = default);


    void Add(Product product);
}