using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Application.ProductVariants;
using Shop.Domain.Common;
using Shop.Domain.Errors;

namespace Shop.Persistence.Queries;

internal sealed class ProductVariantQueries(AppDbContext context) : IProductVariantQueries
{
    public async Task<Maybe<ProductVariantDetailResponse>> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken)
    {
        Result<Slug, Error> slugResult = Slug.Create(slug);
        if (slugResult.IsFailure)
            return Maybe<ProductVariantDetailResponse>.None;

        Slug parsedSlug = slugResult.Value;

        var row = await (
            from variant in context.ProductVariants.AsNoTracking()
            join product in context.Products on variant.ProductId equals product.Id
            join manufacturer in context.Manufacturers
                on product.ManufacturerId equals manufacturer.Id
            join category in context.Categories on product.CategoryId equals category.Id
            where variant.Slug == parsedSlug
            select new
            {
                variant.Id,
                variant.Sku,
                variant.Slug,
                variant.Packaging,
                variant.Price,
                variant.StockQuantity,
                ProductId = product.Id,
                ProductName = product.Name,
                product.Description,
                ManufacturerName = manufacturer.Name,
                manufacturer.Country,
                CategoryName = category.Name
            }).FirstOrDefaultAsync(cancellationToken);

        if (row is null)
            return Maybe<ProductVariantDetailResponse>.None;

        IReadOnlyList<ProductVariantListItemResponse> otherPackagings =
            await LoadVariantsAsync(row.ProductId, row.Id, cancellationToken);

        return new ProductVariantDetailResponse(
            row.Id,
            row.Slug.Value,
            row.Sku,
            $"{row.ProductName} {row.Packaging}",
            row.ProductName,
            row.Description,
            row.ManufacturerName,
            row.Country,
            row.CategoryName,
            row.Packaging.ToString(),
            row.Price.Value,
            row.StockQuantity,
            row.StockQuantity > 0,
            otherPackagings);
    }

    public Task<IReadOnlyList<ProductVariantListItemResponse>> GetByProductAsync(
        Guid productId,
        CancellationToken cancellationToken)
        => LoadVariantsAsync(productId, excludeVariantId: null, cancellationToken);

    private async Task<IReadOnlyList<ProductVariantListItemResponse>> LoadVariantsAsync(
        Guid productId,
        Guid? excludeVariantId,
        CancellationToken cancellationToken)
    {
        IQueryable<Domain.ProductVariants.ProductVariant> query = context.ProductVariants
            .AsNoTracking()
            .Where(v => v.ProductId == productId);

        if (excludeVariantId.HasValue)
            query = query.Where(v => v.Id != excludeVariantId.Value);

        var rows = await query
            .Select(v => new { v.Id, v.Sku, v.Slug, v.Packaging, v.Price, v.StockQuantity })
            .ToListAsync(cancellationToken);

        return rows
            .OrderBy(v => v.Packaging.Unit)
            .ThenBy(v => v.Packaging.Value)
            .Select(v => new ProductVariantListItemResponse(
                v.Id,
                v.Sku,
                v.Slug.Value,
                v.Packaging.ToString(),
                v.Price.Value,
                v.StockQuantity,
                v.StockQuantity > 0))
            .ToList();
    }
}