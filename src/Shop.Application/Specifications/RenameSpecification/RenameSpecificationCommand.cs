namespace Shop.Application.Specifications.RenameSpecification;

public sealed record RenameSpecificationCommand(Guid SpecificationId, string? Name);
