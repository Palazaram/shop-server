namespace Shop.Application.Categories;

public sealed record CategoryTreeItemResponse(
    Guid Id,
    string Name,
    string Slug,
    IReadOnlyList<CategoryTreeItemResponse> Subcategories);

public interface ICategoryQueries
{
    Task<IReadOnlyList<CategoryTreeItemResponse>> GetTreeAsync(CancellationToken cancellationToken);
}