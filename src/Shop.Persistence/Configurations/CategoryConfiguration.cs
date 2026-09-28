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

        builder.Property(c => c.DisplayOrder)
            .IsRequired();

        builder.Property(c => c.MetaTitle)
            .HasMaxLength(Category.MaxMetaTitleLength);

        builder.Property(c => c.MetaDescription)
            .HasMaxLength(Category.MaxMetaDescriptionLength);

        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(c => c.Attributes)
            .WithOne()
            .HasForeignKey(ca => ca.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(c => c.Attributes)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(c => new { c.ParentId, c.Name })
            .IsUnique();

        builder.HasIndex(c => c.Name)
            .IsUnique()
            .HasFilter("parent_id IS NULL");

        // Слаг попадает в адрес страницы, поэтому уникален во всей таблице, а не среди соседей:
        // два раздела с одним слагом означали бы два разных содержимого по одному URL.
        builder.HasIndex(c => c.Slug)
            .IsUnique();
    }
}