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

public interface ICategoryQueries
{
    Task<IReadOnlyList<CategoryTreeItemResponse>> GetTreeAsync(
        CancellationToken cancellationToken);

    Task<Maybe<CategoryAttributesResponse>> GetAttributesAsync(
        Guid categoryId, CancellationToken cancellationToken);
}