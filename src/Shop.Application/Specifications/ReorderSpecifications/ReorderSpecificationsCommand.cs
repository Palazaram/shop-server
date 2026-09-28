namespace Shop.Application.Specifications.ReorderSpecifications;

public sealed record ReorderSpecificationsCommand(IReadOnlyList<Guid>? SpecificationIds);
