using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Options;
using Shop.Application.Products;
using Shop.Application.ProductVariants;
using Shop.Domain.Common;
using Shop.Domain.Errors;
using Shop.Domain.Products;

namespace Shop.Persistence.Queries;

internal sealed class ProductVariantQueries(AppDbContext context, ImageUrlOptions imageUrls)
    : IProductVariantQueries
{
    public async Task<Maybe<ProductVariantDetailResponse>> GetBySlugAsync(string slug, CancellationToken cancellationToken)
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
            join country in context.Countries on manufacturer.CountryId equals country.Id
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
                Country = country.Name,
                CategoryName = category.Name
            }).FirstOrDefaultAsync(cancellationToken);

        if (row is null)
            return Maybe<ProductVariantDetailResponse>.None;

        IReadOnlyList<ProductVariantListItemResponse> otherPackagings =
            await LoadVariantsAsync(row.ProductId, row.Id, cancellationToken);

        IReadOnlyList<ProductImageResponse> images =
            await LoadImagesAsync(row.ProductId, cancellationToken);

        IReadOnlyList<ProductSpecificationResponse> specifications =
            await LoadSpecificationsAsync(row.ProductId, cancellationToken);

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
            images,
            specifications,
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

    /// <summary>
    /// Сначала атрибуты, потом характеристики. Порядок атрибутов — по имени: тонкая настройка
    /// порядка живёт в привязке к категории и нужна сайдбару, а карточке достаточно
    /// предсказуемости. Характеристики идут в порядке справочника.
    /// </summary>
    private async Task<IReadOnlyList<ProductSpecificationResponse>> LoadSpecificationsAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var attributeRows = await (
            from link in context.Set<ProductAttributeValue>().AsNoTracking()
            join value in context.AttributeValues on link.AttributeValueId equals value.Id
            join attribute in context.ProductAttributes on value.AttributeId equals attribute.Id
            where link.ProductId == productId
            select new { AttributeName = attribute.Name, ValueName = value.Name })
            .ToListAsync(cancellationToken);

        var specificationRows = await (
            from link in context.Set<ProductSpecification>().AsNoTracking()
            join specification in context.Specifications
                on link.SpecificationId equals specification.Id
            where link.ProductId == productId
            orderby specification.DisplayOrder
            select new { specification.Name, link.Value })
            .ToListAsync(cancellationToken);

        return
        [
            // У одного атрибута значений может быть несколько — две действующие речовини
            // в строке лучше, чем две строки с одинаковым именем.
            .. attributeRows
                .GroupBy(row => row.AttributeName)
                .OrderBy(group => group.Key, TextComparers.Ukrainian)
                .Select(group => new ProductSpecificationResponse(
                    group.Key,
                    string.Join(", ", group
                        .Select(row => row.ValueName)
                        .OrderBy(name => name, TextComparers.Ukrainian)))),
            .. specificationRows.Select(row =>
                new ProductSpecificationResponse(row.Name, row.Value))
        ];
    }

    private async Task<IReadOnlyList<ProductImageResponse>> LoadImagesAsync(Guid productId, CancellationToken cancellationToken)
    {
        var rows = await context.Set<ProductImage>()
            .AsNoTracking()
            .Where(image => image.ProductId == productId)
            .OrderBy(image => image.DisplayOrder)
            .Select(image => new { image.Id, image.Alt })
            .ToListAsync(cancellationToken);

        return
        [
            .. rows.Select(image => new ProductImageResponse(
                ProductImagePaths.Url(
                    imageUrls.PublicBaseUrl, productId, image.Id, ProductImagePaths.ThumbSize),
                ProductImagePaths.Url(
                    imageUrls.PublicBaseUrl, productId, image.Id, ProductImagePaths.CardSize),
                ProductImagePaths.Url(
                    imageUrls.PublicBaseUrl, productId, image.Id, ProductImagePaths.FullSize),
                image.Alt))
        ];
    }
}