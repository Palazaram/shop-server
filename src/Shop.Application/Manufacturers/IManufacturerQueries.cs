namespace Shop.Application.Manufacturers;

public sealed record ManufacturerResponse(Guid Id, string Name, string Slug, string Country);

public interface IManufacturerQueries
{
    Task<IReadOnlyList<ManufacturerResponse>> GetAllAsync(CancellationToken cancellationToken);
}