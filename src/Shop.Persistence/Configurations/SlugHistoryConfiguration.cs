using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Domain.Common;
using Shop.Domain.SlugHistory;

namespace Shop.Persistence.Configurations;

internal sealed class SlugHistoryConfiguration : IEntityTypeConfiguration<SlugHistoryEntry>
{
    public void Configure(EntityTypeBuilder<SlugHistoryEntry> builder)
    {
        builder.ToTable("slug_history");

        // Внешнего ключа на владельца нет не ради «пережить удаление» — пережившая владельца
        // запись бесполезна, перенаправлять некуда. Причина структурная: ссылка полиморфная,
        // owner_id указывает то в categories, то в product_variants, и одним внешним ключом
        // такое не описать. Судьба записей-сирот решается вместе с удалением сущностей (3.1).

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedNever();

        builder.Property(e => e.OwnerType)
            .HasConversion<string>()
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(e => e.Slug)
            .HasConversion(slug => slug.Value, value => Slug.Create(value).Value)
            .HasMaxLength(Slug.MaxLength)
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .IsRequired();

        // Один прошлый адрес указывает ровно на одну сущность своего вида.
        builder.HasIndex(e => new { e.OwnerType, e.Slug }).IsUnique();

        builder.HasIndex(e => new { e.OwnerType, e.OwnerId });
    }
}
