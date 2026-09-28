using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Domain.Common;
using Shop.Domain.SlugHistory;

namespace Shop.Persistence.Repositories;

internal sealed class SlugHistoryRepository(AppDbContext context) : ISlugHistoryRepository
{
    public async Task<Maybe<SlugHistoryEntry>> FindAsync(
        SlugOwnerType ownerType,
        Slug slug,
        CancellationToken cancellationToken = default)
        => await context.SlugHistory
            .FirstOrDefaultAsync(
                entry => entry.OwnerType == ownerType && entry.Slug == slug,
                cancellationToken);

    public void Add(SlugHistoryEntry entry) => context.SlugHistory.Add(entry);

    public void Remove(SlugHistoryEntry entry) => context.SlugHistory.Remove(entry);
}
