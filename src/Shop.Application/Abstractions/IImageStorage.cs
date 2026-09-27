namespace Shop.Application.Abstractions;

public interface IImageStorage
{
    Task SaveAsync(string key, byte[] content, CancellationToken cancellationToken = default);

    Task DeleteAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken = default);
}