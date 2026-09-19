namespace Shop.Application.AttributeValues.RenameAttributeValue;

public sealed record RenameAttributeValueCommand(Guid ValueId, string? Name);