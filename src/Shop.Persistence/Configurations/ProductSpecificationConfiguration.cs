using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Products;
using Shop.Domain.Specifications;

namespace Shop.Persistence.Configurations;

internal sealed class ProductSpecificationConfiguration
    : IEntityTypeConfiguration<ProductSpecification>
{
    public void Configure(EntityTypeBuilder<ProductSpecification> builder)
    {
        // У дочерней сущности нет DbSet, поэтому имя таблицы задаётся явно.
        builder.ToTable("product_specifications");

        builder.HasKey(ps => new { ps.ProductId, ps.SpecificationId });

        builder.Property(ps => ps.Value)
            .HasMaxLength(ProductSpecification.MaxValueLength)
            .IsRequired();

        builder.HasOne<Specification>()
            .WithMany()
            .HasForeignKey(ps => ps.SpecificationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
