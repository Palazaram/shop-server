using Microsoft.EntityFrameworkCore;
using Shop.Application.Manufacturers;

namespace Shop.Persistence.Queries;

internal sealed class ManufacturerQueries(AppDbContext context) : IManufacturerQueries
{
    public async Task<IReadOnlyList<ManufacturerResponse>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var rows = await context.Manufacturers
            .AsNoTracking()
            .Select(m => new { m.Id, m.Name, m.Slug, m.Country })
            .ToListAsync(cancellationToken);

        return rows
            .OrderBy(row => row.Name, TextComparers.Ukrainian)
            .Select(row => new ManufacturerResponse(
                row.Id, row.Name, row.Slug.Value, row.Country))
            .ToList();
    }
}