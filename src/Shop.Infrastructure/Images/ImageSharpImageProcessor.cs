using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Errors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Shop.Infrastructure.Images;

internal sealed class ImageSharpImageProcessor : IImageProcessor
{
    private const int MaxPixels = 50_000_000;
    private const int WebpQuality = 82;

    // Порядок по убыванию не случаен: каждый следующий размер получается из уже уменьшенного,
    // поэтому картинка обрабатывается одним экземпляром, а не тремя копиями оригинала.
    private static readonly (string Size, int MaxSide)[] Variants =
    [
        ("full", 1600),
        ("card", 600),
        ("thumb", 200)
    ];

    public Result<IReadOnlyList<ImageVariant>, Error> CreateVariants(Stream source)
    {
        try
        {
            // Identify читает только заголовок: размеры известны до того, как картинка
            // будет развёрнута в память. Без этого файл на 400 мегапикселей кладёт процесс.
            ImageInfo info = Image.Identify(source);

            if ((long)info.Width * info.Height > MaxPixels)
                return DomainErrors.Products.ImageResolutionTooLarge(MaxPixels);

            source.Position = 0;

            using Image image = Image.Load(source);

            image.Mutate(context => context.AutoOrient());

            // Снимаем EXIF: там и координаты съёмки, и лишние килобайты в каждой копии.
            image.Metadata.ExifProfile = null;

            // Без явного режима WebpEncoder кодирует PNG без потерь, и Quality не действует:
            // «фото» в 3 МБ превращается в 1.8 МБ вместо ~200 КБ. Lossy умеет альфа-канал,
            // так что прозрачность при этом не теряется.
            var encoder = new WebpEncoder
            {
                Quality = WebpQuality,
                FileFormat = WebpFileFormatType.Lossy
            };

            var results = new List<ImageVariant>(Variants.Length);

            foreach ((string size, int maxSide) in Variants)
            {
                if (image.Width > maxSide || image.Height > maxSide)
                    image.Mutate(context => context.Resize(new ResizeOptions
                    {
                        Mode = ResizeMode.Max,
                        Size = new Size(maxSide, maxSide)
                    }));

                using var buffer = new MemoryStream();
                image.Save(buffer, encoder);

                results.Add(new ImageVariant(size, buffer.ToArray()));
            }

            return results;
        }
        catch (ImageFormatException)
        {
            return DomainErrors.Products.ImageFormatNotSupported();
        }
    }
}