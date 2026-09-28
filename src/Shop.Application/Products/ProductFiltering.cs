using CSharpFunctionalExtensions;
using Shop.Domain.Errors;

namespace Shop.Application.Products;

public sealed record ProductFiltersResponse(
    IReadOnlyList<ProductFilterCategoryResponse> Categories,
    IReadOnlyList<ProductFilterGroupResponse> Groups,
    PriceRangeResponse? PriceRange);

/// <summary>
/// Категория в сайдбаре — дерево, а не чекбоксы: у подразделов есть родитель,
/// и плоский список это скрыл бы. Вне категории отдаются корни с подразделами,
/// внутри категории — её подразделы.
/// </summary>
public sealed record ProductFilterCategoryResponse(
    string Key,
    string Name,
    int ProductCount,
    IReadOnlyList<ProductFilterCategoryResponse> Subcategories);

public sealed record ProductFilterGroupResponse(
    string Key,
    string Name,
    IReadOnlyList<ProductFilterValueResponse> Values);

public sealed record ProductFilterValueResponse(
    string Key,
    string Name,
    int ProductCount);

public sealed record PriceRangeResponse(decimal Min, decimal Max);

public interface IProductFilterQueries
{
    /// <param name="categoryId">null — весь каталог, как и в <see cref="ProductListQuery"/>.</param>
    Task<Result<ProductFiltersResponse, Error>> GetAsync(
        Guid? categoryId,
        ProductFilterSet filters,
        CancellationToken cancellationToken);
}
