using CSharpFunctionalExtensions;
using Shop.Domain.Common;

namespace Shop.Domain.SlugHistory;

public interface ISlugHistoryRepository
{
    Task<Maybe<SlugHistoryEntry>> FindAsync(
        SlugOwnerType ownerType,
        Slug slug,
        CancellationToken cancellationToken = default);

    void Add(SlugHistoryEntry entry);

    void Remove(SlugHistoryEntry entry);
}
