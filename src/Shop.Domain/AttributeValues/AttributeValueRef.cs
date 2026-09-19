namespace Shop.Domain.AttributeValues;

public sealed record AttributeValueRef(Guid ValueId, Guid AttributeId, string ValueName);