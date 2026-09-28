using CSharpFunctionalExtensions;
using Shop.Domain.SlugHistory;

namespace Shop.Application.Catalog;

public interface ISlugHistoryQueries
{
    /// <summary>
    /// Текущий адрес сущности, которой когда-то принадлежал этот слаг. None — если слага
    /// в истории нет или его владелец исчез.
    /// </summary>
    Task<Maybe<string>> FindCurrentSlugAsync(
        SlugOwnerType ownerType,
        string slug,
        CancellationToken cancellationToken);
}
