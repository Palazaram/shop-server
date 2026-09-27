using Shop.Application.Abstractions;
using Shop.Infrastructure.Options;

namespace Shop.Infrastructure.Images;

internal sealed class FileSystemImageStorage(ImageStorageOptions options) : IImageStorage
{
    public async Task SaveAsync(
        string key,
        byte[] content,
        CancellationToken cancellationToken = default)
    {
        string path = ResolvePath(key);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        await File.WriteAllBytesAsync(path, content, cancellationToken);
    }

    public Task DeleteAsync(IReadOnlyList<string> keys, CancellationToken cancellationToken = default)
    {
        foreach (string key in keys)
        {
            string path = ResolvePath(key);

            if (File.Exists(path))
                File.Delete(path);
        }

        return Task.CompletedTask;
    }

    private string ResolvePath(string key)
    {
        string root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(options.RootPath))
            + Path.DirectorySeparatorChar;

        string path = Path.GetFullPath(Path.Combine(root, key));

        if (!path.StartsWith(root, StringComparison.Ordinal))
            throw new InvalidOperationException($"Image key '{key}' resolves outside the storage root.");

        return path;
    }
}