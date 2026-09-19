using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Categories;
using Shop.Domain.Manufacturers;
using Shop.Domain.Products;

namespace Shop.Persistence.Configurations;

internal sealed class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .ValueGeneratedNever();

        builder.Property(p => p.Name)
            .HasMaxLength(Product.MaxNameLength)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(Product.MaxDescriptionLength);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Manufacturer>()
            .WithMany()
            .HasForeignKey(p => p.ManufacturerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.AttributeValues)
            .WithOne()
            .HasForeignKey(pav => pav.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.AttributeValues)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(p => new { p.ManufacturerId, p.Name })
            .IsUnique();

        builder.HasIndex(p => p.CategoryId);
    }
}