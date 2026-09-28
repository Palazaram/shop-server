namespace Shop.Application.Products;

/// <summary>
/// Путь файла выводится из идентификаторов, поэтому имён файлов в базе нет и рассинхрона
/// базы с диском быть не может.
/// </summary>
public static class ProductImagePaths
{
    public const string ThumbSize = "thumb";
    public const string CardSize = "card";
    public const string FullSize = "full";

    public static readonly IReadOnlyList<string> Sizes = [ThumbSize, CardSize, FullSize];

    public static string Key(Guid productId, Guid imageId, string size)
        => $"products/{productId}/{imageId}-{size}.webp";

    public static IReadOnlyList<string> AllKeys(Guid productId, Guid imageId)
        => [.. Sizes.Select(size => Key(productId, imageId, size))];

    public static string Url(string publicBaseUrl, Guid productId, Guid imageId, string size)
        => $"{publicBaseUrl.TrimEnd('/')}/{Key(productId, imageId, size)}";
}