using Microsoft.EntityFrameworkCore;
using Shop.Application.Countries;

namespace Shop.Persistence.Queries;

internal sealed class CountryQueries(AppDbContext context) : ICountryQueries
{
    public async Task<IReadOnlyList<CountryResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var rows = await context.Countries
            .AsNoTracking()
            .Select(c => new { c.Id, c.Name, c.Slug })
            .ToListAsync(cancellationToken);

        return rows
            .OrderBy(row => row.Name, TextComparers.Ukrainian)
            .Select(row => new CountryResponse(row.Id, row.Name, row.Slug.Value))
            .ToList();
    }
}