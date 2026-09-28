using Microsoft.EntityFrameworkCore;
using Shop.Application.Specifications;

namespace Shop.Persistence.Queries;

internal sealed class SpecificationQueries(AppDbContext context) : ISpecificationQueries
{
    public async Task<IReadOnlyList<SpecificationResponse>> GetAllAsync(
        CancellationToken cancellationToken)
        => await context.Specifications
            .AsNoTracking()
            .OrderBy(s => s.DisplayOrder)
            .Select(s => new SpecificationResponse(s.Id, s.Name, s.DisplayOrder))
            .ToListAsync(cancellationToken);
}
