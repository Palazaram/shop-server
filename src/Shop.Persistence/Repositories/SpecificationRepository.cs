using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Domain.Specifications;

namespace Shop.Persistence.Repositories;

internal sealed class SpecificationRepository(AppDbContext context) : ISpecificationRepository
{
    public async Task<Maybe<Specification>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
        => await context.Specifications.FindAsync([id], cancellationToken);

    public async Task<IReadOnlyList<Specification>> GetAllAsync(
        CancellationToken cancellationToken = default)
        => await context.Specifications
            .OrderBy(s => s.DisplayOrder)
            .ToListAsync(cancellationToken);

    public Task<bool> ExistsByNameAsync(
        string name,
        Guid? excludeSpecificationId,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Specification> query = context.Specifications.Where(s => s.Name == name);

        if (excludeSpecificationId.HasValue)
            query = query.Where(s => s.Id != excludeSpecificationId.Value);

        return query.AnyAsync(cancellationToken);
    }

    public async Task<bool> AllExistAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
            return true;

        int found = await context.Specifications
            .CountAsync(s => ids.Contains(s.Id), cancellationToken);

        return found == ids.Count;
    }

    public async Task<int> GetNextDisplayOrderAsync(CancellationToken cancellationToken = default)
    {
        int? last = await context.Specifications
            .MaxAsync(s => (int?)s.DisplayOrder, cancellationToken);

        return (last ?? -1) + 1;
    }

    public void Add(Specification specification) => context.Specifications.Add(specification);
}
