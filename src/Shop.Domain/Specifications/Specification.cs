using CSharpFunctionalExtensions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Errors;

namespace Shop.Domain.Specifications;

/// <summary>
/// Имя характеристики из справочника: «Клас токсичності», «Період очікування». Слага нет
/// намеренно — по характеристикам не фильтруют, и в адрес они не попадают. Порядок показа
/// глобальный: карточка выводит те характеристики, которые у товара заполнены, поэтому
/// привязка к категории (как у атрибутов) здесь ничего не решала бы.
/// </summary>
public sealed class Specification : AggregateRoot<Guid>
{
    public const int MaxNameLength = 100;

    private Specification(Guid id, string name, int displayOrder) : base(id)
    {
        Name = name;
        DisplayOrder = displayOrder;
    }

    private Specification()
    {
    }

    public string Name { get; private set; } = null!;
    public int DisplayOrder { get; private set; }

    public static Result<Specification, Error> Create(string? name, int displayOrder)
    {
        Result<string, Error> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return normalizedName.Error;

        return new Specification(Guid.CreateVersion7(), normalizedName.Value, displayOrder);
    }

    public UnitResult<Error> Rename(string? name)
    {
        Result<string, Error> normalizedName = NormalizeName(name);
        if (normalizedName.IsFailure)
            return normalizedName.Error;

        Name = normalizedName.Value;
        return UnitResult.Success<Error>();
    }

    public void SetDisplayOrder(int displayOrder) => DisplayOrder = displayOrder;

    private static Result<string, Error> NormalizeName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return DomainErrors.Specifications.NameIsRequired();

        string normalized = name.CollapseWhitespace();

        if (normalized.Length > MaxNameLength)
            return DomainErrors.Specifications.NameTooLong(MaxNameLength);

        return normalized;
    }
}
