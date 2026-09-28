using Microsoft.Extensions.FileProviders;
using Shop.Application.Options;
using Shop.Infrastructure.Options;

namespace Shop.Api.Extensions;

public static class ImageHostingExtensions
{
    /// <summary>
    /// Путь монтирования статики. Намеренно не берётся из PublicBaseUrl: тот может стать
    /// абсолютным адресом CDN, а RequestPath обязан быть путём внутри приложения.
    /// </summary>
    private const string ImageRequestPath = "/images";

    public static WebApplication UseProductImages(this WebApplication app)
    {
        var storage = app.Services.GetRequiredService<ImageStorageOptions>();

        var urls = app.Services.GetRequiredService<ImageUrlOptions>();

        string publicBaseUrl = urls.PublicBaseUrl.TrimEnd('/');

        if (publicBaseUrl.StartsWith('/')
            && !string.Equals(publicBaseUrl, ImageRequestPath, StringComparison.Ordinal))
        {
            app.Logger.LogWarning(
                "Images:PublicBaseUrl is {PublicBaseUrl}, but images are served from {RequestPath}. "
                + "Image links will not open: use an absolute CDN URL or match the serving path.",
                urls.PublicBaseUrl, ImageRequestPath);
        }

        Directory.CreateDirectory(storage.RootPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(storage.RootPath),
            RequestPath = ImageRequestPath,
            // Имя файла содержит id изображения и никогда не переиспользуется.
            OnPrepareResponse = context =>
                context.Context.Response.Headers.CacheControl = "public,max-age=604800,immutable"
        });

        return app;
    }
}