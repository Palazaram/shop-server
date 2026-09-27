using Microsoft.Extensions.FileProviders;
using Shop.Infrastructure.Options;

namespace Shop.Api.Extensions;

public static class ImageHostingExtensions
{
    public static WebApplication UseProductImages(this WebApplication app)
    {
        var options = app.Services.GetRequiredService<ImageStorageOptions>();

        Directory.CreateDirectory(options.RootPath);

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(options.RootPath),
            RequestPath = options.PublicBaseUrl,
            // Имя файла содержит id изображения и никогда не переиспользуется,
            // поэтому содержимое по адресу не меняется — можно кэшировать надолго.
            OnPrepareResponse = context =>
                context.Context.Response.Headers.CacheControl = "public,max-age=604800,immutable"
        });

        return app;
    }
}