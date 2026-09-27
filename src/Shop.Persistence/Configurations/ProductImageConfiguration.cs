using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Products;

namespace Shop.Persistence.Configurations;

internal sealed class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(EntityTypeBuilder<ProductImage> builder)
    {
        builder.ToTable("product_images");

        builder.HasKey(image => image.Id);

        builder.Property(image => image.Id)
            .ValueGeneratedNever();

        builder.Property(image => image.Alt)
            .HasMaxLength(ProductImage.MaxAltLength);

        builder.HasIndex(image => new { image.ProductId, image.DisplayOrder });
    }
}