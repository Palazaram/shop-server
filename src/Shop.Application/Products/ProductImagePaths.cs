namespace Shop.Application.Products;

/// <summary>
/// Путь файла выводится из идентификаторов, поэтому имён файлов в базе нет и рассинхрона
/// базы с диском быть не может.
/// </summary>
public static class ProductImagePaths
{
    public static readonly IReadOnlyList<string> Sizes = ["thumb", "card", "full"];

    public static string Key(Guid productId, Guid imageId, string size)
        => $"products/{productId}/{imageId}-{size}.webp";

    public static IReadOnlyList<string> AllKeys(Guid productId, Guid imageId)
        => [.. Sizes.Select(size => Key(productId, imageId, size))];
}