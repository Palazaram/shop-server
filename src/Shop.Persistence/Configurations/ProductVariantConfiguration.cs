using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Common;
using Shop.Domain.ProductVariants;
using Shop.Domain.Products;

namespace Shop.Persistence.Configurations;

internal sealed class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .ValueGeneratedNever();

        builder.Property(v => v.Sku)
            .HasMaxLength(ProductVariant.MaxSkuLength)
            .IsRequired();

        builder.Property(v => v.Slug)
            .HasConversion(slug => slug.Value, value => Slug.Create(value).Value)
            .HasMaxLength(Slug.MaxLength)
            .IsRequired();

        builder.ComplexProperty(v => v.Price, price =>
        {
            price.Property(p => p.Value)
                .HasColumnName("price")
                .HasPrecision(18, 2)
                .IsRequired();
        });

        builder.ComplexProperty(v => v.Packaging, packaging =>
        {
            packaging.Property(p => p.Value)
                .HasColumnName("packaging_value")
                .HasPrecision(18, 3)
                .IsRequired();

            packaging.Property(p => p.Unit)
                .HasColumnName("packaging_unit")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            packaging.Property(p => p.Key)
                .HasColumnName("packaging_key")
                .HasMaxLength(40)
                .IsRequired();
        });

        builder.HasOne<Product>()
            .WithMany()
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => v.Sku).IsUnique();
        builder.HasIndex(v => v.Slug).IsUnique();
        builder.HasIndex(v => v.ProductId);
    }
}