using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Errors;
using Shop.Domain.Specifications;

namespace Shop.Application.Specifications.ReorderSpecifications;

internal sealed class ReorderSpecificationsCommandHandler(
    ISpecificationRepository specificationRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<ReorderSpecificationsCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        ReorderSpecificationsCommand command,
        CancellationToken cancellationToken)
    {
        IReadOnlyList<Guid> ids = command.SpecificationIds!;

        IReadOnlyList<Specification> all =
            await specificationRepository.GetAllAsync(cancellationToken);

        // Список обязан содержать справочник целиком — как у категорий и картинок:
        // частичная перестановка оставила бы порядок неопределённым.
        if (ids.Count != all.Count)
            return DomainErrors.Specifications.OrderIsIncomplete(all.Count);

        Dictionary<Guid, Specification> byId = all.ToDictionary(s => s.Id);

        foreach (Guid id in ids)
            if (!byId.ContainsKey(id))
                return DomainErrors.Specifications.UnknownIdInOrder(id);

        for (int index = 0; index < ids.Count; index++)
            byId[ids[index]].SetDisplayOrder(index);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}
