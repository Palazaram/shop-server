using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.RefreshTokens;
using Shop.Domain.Users;

namespace Shop.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.Id)
            .ValueGeneratedNever();

        builder.Property(rt => rt.TokenHash)
            .HasConversion(
                tokenHash => tokenHash.Value,
                value => TokenHash.Create(value).Value)
            .HasColumnType("text")
            .IsRequired();

        builder.HasIndex(rt => rt.TokenHash)
           .IsUnique();

        builder.HasIndex(rt => rt.UserId);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
