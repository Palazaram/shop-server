namespace Shop.Application.Specifications;

public sealed record SpecificationResponse(Guid Id, string Name, int DisplayOrder);

public interface ISpecificationQueries
{
    Task<IReadOnlyList<SpecificationResponse>> GetAllAsync(CancellationToken cancellationToken);
}
