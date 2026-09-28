namespace Shop.Infrastructure.Options;

public sealed class ImageStorageOptions
{
    public const string SectionName = "Images";

    /// <summary>В конфигурации путь относительный; при регистрации он превращается в абсолютный.</summary>
    public string RootPath { get; set; } = "App_Data/images";
}