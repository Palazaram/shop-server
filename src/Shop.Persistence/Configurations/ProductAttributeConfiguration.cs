using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Common;
using Shop.Domain.ProductAttributes;

namespace Shop.Persistence.Configurations;

internal sealed class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
{
    public void Configure(EntityTypeBuilder<ProductAttribute> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id)
            .ValueGeneratedNever();

        builder.Property(a => a.Name)
            .HasMaxLength(ProductAttribute.MaxNameLength)
            .IsRequired();

        builder.Property(a => a.Slug)
            .HasConversion(slug => slug.Value, value => Slug.Create(value).Value)
            .HasMaxLength(Slug.MaxLength)
            .IsRequired();

        builder.HasIndex(a => a.Name).IsUnique();
        builder.HasIndex(a => a.Slug).IsUnique();
    }
}