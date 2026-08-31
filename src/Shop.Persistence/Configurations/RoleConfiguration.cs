using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Roles;

namespace Shop.Persistence.Configurations;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.Name)
            .HasMaxLength(Role.MaxNameLength)
            .IsRequired();

        builder.HasData(
            Role.Create(RoleIds.Admin, RoleNames.Admin),
            Role.Create(RoleIds.Customer, RoleNames.Customer));
    }
}
