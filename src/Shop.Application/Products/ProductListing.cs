using CSharpFunctionalExtensions;
using Shop.Domain.Errors;

namespace Shop.Application.Products;

public sealed record ProductListFilter(
    string AttributeSlug,
    IReadOnlyList<string> ValueSlugs);

public sealed record ProductListQuery(
    Guid CategoryId,
    IReadOnlyList<ProductListFilter> Filters,
    string? Sort,
    int Page,
    int PageSize)
{
    public const int DefaultPageSize = 24;
    public const int MaxPageSize = 60;
}

public sealed record ProductListItemResponse(
    Guid VariantId,
    Guid ProductId,
    string Name,
    string Slug,
    string Sku,
    decimal Price,
    int StockQuantity,
    string ManufacturerName);

public sealed record ProductListResponse(
    IReadOnlyList<ProductListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages);

public interface IProductListQueries
{
    Task<Result<ProductListResponse, Error>> ListAsync(
        ProductListQuery query, CancellationToken cancellationToken);
}