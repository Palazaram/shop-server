namespace Shop.Application.Manufacturers;

public sealed record ManufacturerResponse(
    Guid Id, 
    string Name, 
    string Slug, 
    Guid CountryId, 
    string Country);

public interface IManufacturerQueries
{
    Task<IReadOnlyList<ManufacturerResponse>> GetAllAsync(CancellationToken cancellationToken);
}