using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.AttributeValues;
using Shop.Domain.Common;
using Shop.Domain.Errors;
using Shop.Domain.ProductAttributes;

namespace Shop.Application.AttributeValues.CreateAttributeValue;

internal sealed class CreateAttributeValueCommandHandler(
    IAttributeValueRepository valueRepository,
    IProductAttributeRepository attributeRepository,
    ISlugGenerator slugGenerator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateAttributeValueCommand, CreateAttributeValueResponse>
{
    public async Task<Result<CreateAttributeValueResponse, Error>> HandleAsync(
        CreateAttributeValueCommand command,
        CancellationToken cancellationToken)
    {
        if (!await attributeRepository.ExistsAsync(command.AttributeId, cancellationToken))
            return DomainErrors.AttributeValues.AttributeNotFound();

        bool slugProvided = !string.IsNullOrWhiteSpace(command.Slug);

        string slugSource = slugProvided
            ? command.Slug!
            : slugGenerator.Generate(command.Name);

        Result<Slug, Error> slugResult = Slug.Create(slugSource);
        if (slugResult.IsFailure)
            return slugProvided
                ? slugResult.Error
                : DomainErrors.AttributeValues.SlugCannotBeGenerated();

        Result<AttributeValue, Error> valueResult = AttributeValue.Create(
            attributeId: command.AttributeId,
            name: command.Name,
            slug: slugResult.Value);

        if (valueResult.IsFailure)
            return valueResult.Error;

        AttributeValue value = valueResult.Value;

        if (await valueRepository.ExistsByNameAsync(
                value.AttributeId, value.Name, null, cancellationToken))
            return DomainErrors.AttributeValues.NameAlreadyExists();

        if (await valueRepository.ExistsBySlugAsync(
                value.AttributeId, value.Slug, cancellationToken))
            return slugProvided
                ? DomainErrors.AttributeValues.SlugAlreadyExists()
                : DomainErrors.AttributeValues.GeneratedSlugAlreadyExists();

        valueRepository.Add(value);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateAttributeValueResponse(value.Id, value.Slug.Value);
    }
}