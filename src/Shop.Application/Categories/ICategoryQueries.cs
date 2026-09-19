using CSharpFunctionalExtensions;

namespace Shop.Application.Categories;

public sealed record CategoryTreeItemResponse(
    Guid Id,
    string Name,
    string Slug,
    IReadOnlyList<CategoryTreeItemResponse> Subcategories);

public sealed record CategoryAttributeItemResponse(Guid Id, string Name, string Slug);

public sealed record CategoryAttributesResponse(
    bool IsInherited,
    Guid? InheritedFromCategoryId,
    IReadOnlyList<CategoryAttributeItemResponse> Attributes);

public sealed record CategoryFiltersResponse(
    IReadOnlyList<CategoryFilterGroupResponse> Groups);

public sealed record CategoryFilterGroupResponse(
    Guid AttributeId,
    string Name,
    string Slug,
    IReadOnlyList<CategoryFilterValueResponse> Values);

public sealed record CategoryFilterValueResponse(
    Guid ValueId,
    string Name,
    string Slug,
    int ProductCount);

public interface ICategoryQueries
{
    Task<IReadOnlyList<CategoryTreeItemResponse>> GetTreeAsync(
        CancellationToken cancellationToken);

    Task<Maybe<CategoryAttributesResponse>> GetAttributesAsync(
        Guid categoryId, CancellationToken cancellationToken);

    Task<Maybe<CategoryFiltersResponse>> GetFiltersAsync(
        Guid categoryId, CancellationToken cancellationToken);
}