using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Categories;
using Shop.Domain.ProductAttributes;

namespace Shop.Persistence.Configurations;

internal sealed class CategoryAttributeConfiguration : IEntityTypeConfiguration<CategoryAttribute>
{
    public void Configure(EntityTypeBuilder<CategoryAttribute> builder)
    {
        builder.ToTable("category_attributes");
        builder.HasKey(ca => new { ca.CategoryId, ca.AttributeId });

        builder.Property(ca => ca.DisplayOrder).IsRequired();

        builder.HasOne<ProductAttribute>()
            .WithMany()
            .HasForeignKey(ca => ca.AttributeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}