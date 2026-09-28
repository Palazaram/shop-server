using Shop.Domain.Abstractions;
using Shop.Domain.Common;

namespace Shop.Domain.SlugHistory;

/// <summary>
/// Прошлый адрес сущности. Заводится при смене слага и живёт, пока этот адрес никем не занят:
/// ссылки и выдача поисковика переживают переименование, а «живая» запись всегда сильнее
/// истории — иначе один адрес вёл бы и к новому владельцу, и к старому через редирект.
/// </summary>
public sealed class SlugHistoryEntry : AggregateRoot<Guid>
{
    private SlugHistoryEntry(
        Guid id,
        SlugOwnerType ownerType,
        Guid ownerId,
        Slug slug,
        DateTimeOffset createdAt) : base(id)
    {
        OwnerType = ownerType;
        OwnerId = ownerId;
        Slug = slug;
        CreatedAt = createdAt;
    }

    private SlugHistoryEntry()
    {
    }

    public SlugOwnerType OwnerType { get; private set; }
    public Guid OwnerId { get; private set; }
    public Slug Slug { get; private set; } = null!;
    public DateTimeOffset CreatedAt { get; private set; }

    public static SlugHistoryEntry Create(
        SlugOwnerType ownerType,
        Guid ownerId,
        Slug slug,
        DateTimeOffset createdAt)
    {
        ArgumentNullException.ThrowIfNull(slug);

        if (ownerId == Guid.Empty)
            throw new ArgumentException("Owner id must not be empty.", nameof(ownerId));

        return new SlugHistoryEntry(Guid.CreateVersion7(), ownerType, ownerId, slug, createdAt);
    }
}
