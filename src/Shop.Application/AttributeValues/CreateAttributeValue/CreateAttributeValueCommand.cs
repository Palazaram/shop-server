namespace Shop.Application.AttributeValues.CreateAttributeValue;

public sealed record CreateAttributeValueCommand(Guid AttributeId, string? Name, string? Slug);