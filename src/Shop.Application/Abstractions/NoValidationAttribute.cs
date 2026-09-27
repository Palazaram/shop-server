namespace Shop.Application.Abstractions;

/// <summary>
/// Помечает контракт, которому валидатор не нужен.
/// Проверка на старте пропускает только помеченные явно.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class NoValidationAttribute : Attribute;