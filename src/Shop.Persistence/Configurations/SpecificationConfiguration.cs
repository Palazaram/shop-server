using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Specifications;

namespace Shop.Persistence.Configurations;

internal sealed class SpecificationConfiguration : IEntityTypeConfiguration<Specification>
{
    public void Configure(EntityTypeBuilder<Specification> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .ValueGeneratedNever();

        builder.Property(s => s.Name)
            .HasMaxLength(Specification.MaxNameLength)
            .IsRequired();

        builder.HasIndex(s => s.Name).IsUnique();
    }
}
