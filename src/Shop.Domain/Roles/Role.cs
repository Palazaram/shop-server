using Shop.Domain.Abstractions;

namespace Shop.Domain.Roles;

public sealed class Role : AggregateRoot<int>
{
    private const int MaxNameLength = 50;

    public string Name { get; private set; } = default!;

    private Role() { }

    private Role(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public static Role Create(int id, string name)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name must not be empty.", nameof(name));

        string normalized = name.Trim();

        if (normalized.Length > MaxNameLength)
            throw new ArgumentException($"Role name must not exceed {MaxNameLength} characters.", nameof(name));

        return new Role(id, normalized);
    }
}