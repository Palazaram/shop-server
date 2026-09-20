using CSharpFunctionalExtensions;
using Shop.Application.Products;
using Shop.Domain.Errors;

namespace Shop.Application.Categories;

public sealed record CategoryFiltersResponse(
    IReadOnlyList<CategoryFilterGroupResponse> Groups,
    PriceRangeResponse? PriceRange);

public sealed record CategoryFilterGroupResponse(
    string Key,
    string Name,
    IReadOnlyList<CategoryFilterValueResponse> Values);

public sealed record CategoryFilterValueResponse(
    string Key,
    string Name,
    int ProductCount);

public sealed record PriceRangeResponse(decimal Min, decimal Max);

public interface ICategoryFilterQueries
{
    Task<Result<CategoryFiltersResponse, Error>> GetAsync(
        Guid categoryId,
        ProductFilterSet filters,
        CancellationToken cancellationToken);
}