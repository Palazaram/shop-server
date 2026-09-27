using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Common;
using Shop.Domain.Countries;

namespace Shop.Persistence.Configurations;

internal sealed class CountryConfiguration : IEntityTypeConfiguration<Country>
{
    public void Configure(EntityTypeBuilder<Country> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();

        builder.Property(c => c.Name)
            .HasMaxLength(Country.MaxNameLength)
            .IsRequired();

        builder.Property(c => c.Slug)
            .HasConversion(slug => slug.Value, value => Slug.Create(value).Value)
            .HasMaxLength(Slug.MaxLength)
            .IsRequired();

        builder.HasIndex(c => c.Name).IsUnique();
        builder.HasIndex(c => c.Slug).IsUnique();
    }
}