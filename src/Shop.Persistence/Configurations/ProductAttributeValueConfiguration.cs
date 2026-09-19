using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.AttributeValues;
using Shop.Domain.Products;

namespace Shop.Persistence.Configurations;

internal sealed class ProductAttributeValueConfiguration
    : IEntityTypeConfiguration<ProductAttributeValue>
{
    public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
    {
        builder.ToTable("product_attribute_values");

        builder.HasKey(pav => new { pav.ProductId, pav.AttributeValueId });

        builder.HasOne<AttributeValue>()
            .WithMany()
            .HasForeignKey(pav => pav.AttributeValueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}