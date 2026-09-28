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

        // Умолчание остаётся в схеме намеренно, вопреки общему правилу «снять после заполнения»:
        // в эту таблицу пишет не только приложение (tools/seed-agro.sql вставляет товары
        // перечислением колонок), а false здесь — не «пустое значение», а законное состояние.
        builder.Property(p => p.IsFeatured)
            .HasDefaultValue(false);

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

        builder.HasMany(p => p.Images)
            .WithOne()
            .HasForeignKey(image => image.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.Images)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(p => p.Specifications)
            .WithOne()
            .HasForeignKey(ps => ps.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(p => p.AttributeValues)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Navigation(p => p.Specifications)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(p => new { p.ManufacturerId, p.Name })
            .IsUnique();
        builder.HasIndex(p => p.CategoryId);

        // Частичный: главная спрашивает только отмеченные, и их единицы.
        builder.HasIndex(p => p.IsFeatured)
            .HasFilter("is_featured");

        builder.HasIndex(p => p.Name, "ix_products_name_trgm")
            .HasDatabaseName("ix_products_name_trgm")
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
    }
}