using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.AttributeValues;
using Shop.Domain.Common;
using Shop.Domain.Errors;

namespace Shop.Application.AttributeValues.ChangeAttributeValueSlug;

internal sealed class ChangeAttributeValueSlugCommandHandler(
    IAttributeValueRepository valueRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<ChangeAttributeValueSlugCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        ChangeAttributeValueSlugCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<AttributeValue> maybeValue =
            await valueRepository.GetByIdAsync(command.ValueId, cancellationToken);

        if (maybeValue.HasNoValue)
            return DomainErrors.AttributeValues.NotFound();

        Result<Slug, Error> slugResult = Slug.Create(command.Slug);
        if (slugResult.IsFailure)
            return slugResult.Error;

        AttributeValue value = maybeValue.Value;

        // Область уникальности — свой атрибут, а не вся таблица.
        if (await valueRepository.ExistsBySlugAsync(
                value.AttributeId, slugResult.Value, value.Id, cancellationToken))
            return DomainErrors.AttributeValues.SlugAlreadyExists();

        value.ChangeSlug(slugResult.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}