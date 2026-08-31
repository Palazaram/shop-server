namespace Shop.Persistence.Exceptions;

public sealed class DuplicateKeyException(string? constraintName, Exception innerException)
    : Exception($"Unique constraint violated: {constraintName}", innerException)
{
    public string? ConstraintName { get; } = constraintName;
}