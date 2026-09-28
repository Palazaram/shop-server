namespace Shop.Application.Options;

public sealed class ImageUrlOptions
{
    public const string SectionName = "Images";

    /// <summary>
    /// Адрес, по которому картинки видны снаружи: «/images» сейчас, адрес CDN потом.
    /// Это не путь на диске — им ведает ImageStorageOptions в инфраструктуре.
    /// </summary>
    public string PublicBaseUrl { get; init; } = "/images";
}