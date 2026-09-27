using Microsoft.EntityFrameworkCore;
using Shop.Application.Manufacturers;

namespace Shop.Persistence.Queries;

internal sealed class ManufacturerQueries(AppDbContext context) : IManufacturerQueries
{
    public async Task<IReadOnlyList<ManufacturerResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var rows = await (
            from manufacturer in context.Manufacturers.AsNoTracking()
            join country in context.Countries on manufacturer.CountryId equals country.Id
            select new
            {
                manufacturer.Id,
                manufacturer.Name,
                manufacturer.Slug,
                CountryId = country.Id,
                CountryName = country.Name
            })
            .ToListAsync(cancellationToken);

        return rows
            .OrderBy(row => row.Name, TextComparers.Ukrainian)
            .Select(row => new ManufacturerResponse(
                row.Id, row.Name, row.Slug.Value, row.CountryId, row.CountryName))
            .ToList();
    }
}