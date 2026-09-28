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

/// <summary>
/// Шапка страницы категории. Нужна фронту ещё и затем, чтобы превратить слаг из адреса
/// в id: листинг и сайдбар принимают id, а в адресе у фронта только слаг.
/// </summary>
public sealed record CategoryHeaderResponse(
    Guid Id,
    string Name,
    string Slug,
    Guid? ParentId,
    string? ParentName,
    string? ParentSlug,
    string? MetaTitle,
    string? MetaDescription);

public interface ICategoryQueries
{
    Task<Maybe<CategoryHeaderResponse>> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<CategoryTreeItemResponse>> GetTreeAsync(
        CancellationToken cancellationToken);

    Task<Maybe<CategoryAttributesResponse>> GetAttributesAsync(
        Guid categoryId, CancellationToken cancellationToken);
}