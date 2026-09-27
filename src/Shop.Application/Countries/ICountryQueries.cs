namespace Shop.Application.Countries;

public sealed record CountryResponse(Guid Id, string Name, string Slug);

public interface ICountryQueries
{
    Task<IReadOnlyList<CountryResponse>> GetAllAsync(CancellationToken cancellationToken);
}