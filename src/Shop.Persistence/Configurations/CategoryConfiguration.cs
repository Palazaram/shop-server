using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Categories;
using Shop.Domain.Common;

namespace Shop.Persistence.Configurations;

internal sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Name)
            .HasMaxLength(Category.MaxNameLength)
            .IsRequired();

        builder.Property(c => c.Slug)
            .HasConversion(slug => slug.Value, value => Slug.Create(value).Value)
            .HasMaxLength(Slug.MaxLength)
            .IsRequired();

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.ParentId, c.Name })
            .IsUnique();

        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasFilter("parent_id IS NULL");

        builder.HasIndex(c => new { c.ParentId, c.Slug })
            .IsUnique();

        builder.HasIndex(c => c.Slug)
            .IsUnique()
            .HasFilter("parent_id IS NULL");
    }
}