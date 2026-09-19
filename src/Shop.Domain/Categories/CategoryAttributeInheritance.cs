namespace Shop.Domain.Categories;

public static class CategoryAttributeInheritance
{
    /// <summary>
    /// Возвращает id категории, чей набор атрибутов действует для указанной.
    /// Совпадает с самой категорией — набор свой; отличается — унаследован.
    /// </summary>
    public static Guid ResolveOwner(Guid categoryId, Guid? parentId, bool hasOwnAttributes)
        => hasOwnAttributes || parentId is null
            ? categoryId
            : parentId.Value;
}