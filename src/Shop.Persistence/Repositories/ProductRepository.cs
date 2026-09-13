using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Domain.Products;

namespace Shop.Persistence.Repositories;

internal sealed class ProductRepository(AppDbContext context) : IProductRepository
{
    public async Task<Maybe<Product>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await context.Products.FindAsync([id], cancellationToken);

    public Task<bool> ExistsByNameAsync(
        string name,
        Guid manufacturerId,
        Guid? excludeProductId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Product> query = context.Products
            .Where(p => p.Name == name && p.ManufacturerId == manufacturerId);

        if (excludeProductId.HasValue)
            query = query.Where(p => p.Id != excludeProductId.Value);

        return query.AnyAsync(cancellationToken);
    }

    public void Add(Product product) => context.Products.Add(product);
}