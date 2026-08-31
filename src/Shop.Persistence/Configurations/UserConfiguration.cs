using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Roles;
using Shop.Domain.Users;

namespace Shop.Persistence.Configurations;

internal sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        builder.ComplexProperty(u => u.FullName, fullName =>
        {
            fullName.Property(x => x.FirstName)
                .HasMaxLength(FullName.MaxLength)
                .IsRequired();

            fullName.Property(x => x.LastName)
                .HasMaxLength(FullName.MaxLength)
                .IsRequired();

            fullName.Property(x => x.Patronymic)
                .HasMaxLength(FullName.MaxLength)
                .IsRequired();
        });

        builder.Property(u => u.PasswordHash)
            .HasConversion(
                passwordHash => passwordHash.Value,
                value => PasswordHash.Create(value).Value)
            .HasColumnType("text")
            .IsRequired();

        builder.Property(u => u.Email)
            .HasConversion(
                email => email.Value,
                value => Email.Create(value).Value)
            .HasMaxLength(Email.MaxLength)
            .IsRequired();

        builder.Property(u => u.Phone)
            .HasConversion(
                phone => phone.Value,
                value => Phone.Create(value).Value)
            .HasMaxLength(Phone.NationalNumberLength)
            .IsRequired();

        builder.HasOne<Role>()
            .WithMany()
            .HasForeignKey(u => u.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasIndex(u => u.Phone)
            .IsUnique();
    }
}
