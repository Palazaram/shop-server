using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Common;
using Shop.Domain.Manufacturers;

namespace Shop.Persistence.Configurations;

internal sealed class ManufacturerConfiguration : IEntityTypeConfiguration<Manufacturer>
{
    public void Configure(EntityTypeBuilder<Manufacturer> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .ValueGeneratedNever();

        builder.Property(m => m.Name)
            .HasMaxLength(Manufacturer.MaxNameLength)
            .IsRequired();

        builder.Property(m => m.Country)
            .HasMaxLength(Manufacturer.MaxCountryLength)
            .IsRequired();

        builder.Property(m => m.Slug)
            .HasConversion(slug => slug.Value, value => Slug.Create(value).Value)
            .HasMaxLength(Slug.MaxLength)
            .IsRequired();

        builder.HasIndex(m => m.Name).IsUnique();
        builder.HasIndex(m => m.Slug).IsUnique();
    }
}