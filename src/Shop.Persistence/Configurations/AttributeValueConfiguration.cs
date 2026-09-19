using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.AttributeValues;
using Shop.Domain.Common;
using Shop.Domain.ProductAttributes;

namespace Shop.Persistence.Configurations;

internal sealed class AttributeValueConfiguration : IEntityTypeConfiguration<AttributeValue>
{
    public void Configure(EntityTypeBuilder<AttributeValue> builder)
    {
        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .ValueGeneratedNever();

        builder.Property(v => v.Name)
            .HasMaxLength(AttributeValue.MaxNameLength)
            .IsRequired();

        builder.Property(v => v.Slug)
            .HasConversion(slug => slug.Value, value => Slug.Create(value).Value)
            .HasMaxLength(Slug.MaxLength)
            .IsRequired();

        builder.HasOne<ProductAttribute>()
            .WithMany()
            .HasForeignKey(v => v.AttributeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(v => new { v.AttributeId, v.Name }).IsUnique();
        builder.HasIndex(v => new { v.AttributeId, v.Slug }).IsUnique();
    }
}