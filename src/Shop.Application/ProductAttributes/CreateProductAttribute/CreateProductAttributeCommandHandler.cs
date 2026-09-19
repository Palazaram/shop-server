using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Common;
using Shop.Domain.Errors;
using Shop.Domain.ProductAttributes;

namespace Shop.Application.ProductAttributes.CreateProductAttribute;

internal sealed class CreateProductAttributeCommandHandler(
    IProductAttributeRepository attributeRepository,
    ISlugGenerator slugGenerator,
    IUnitOfWork unitOfWork)
    : ICommandHandler<CreateProductAttributeCommand, CreateProductAttributeResponse>
{
    public async Task<Result<CreateProductAttributeResponse, Error>> HandleAsync(
        CreateProductAttributeCommand command,
        CancellationToken cancellationToken)
    {
        bool slugProvided = !string.IsNullOrWhiteSpace(command.Slug);

        string slugSource = slugProvided
            ? command.Slug!
            : slugGenerator.Generate(command.Name);

        Result<Slug, Error> slugResult = Slug.Create(slugSource);
        if (slugResult.IsFailure)
            return slugProvided
                ? slugResult.Error
                : DomainErrors.ProductAttributes.SlugCannotBeGenerated();

        Result<ProductAttribute, Error> attributeResult =
            ProductAttribute.Create(command.Name, slugResult.Value);

        if (attributeResult.IsFailure)
            return attributeResult.Error;

        ProductAttribute attribute = attributeResult.Value;

        if (await attributeRepository.ExistsByNameAsync(attribute.Name, null, cancellationToken))
            return DomainErrors.ProductAttributes.NameAlreadyExists();

        if (await attributeRepository.ExistsBySlugAsync(attribute.Slug, cancellationToken))
            return slugProvided
                ? DomainErrors.ProductAttributes.SlugAlreadyExists()
                : DomainErrors.ProductAttributes.GeneratedSlugAlreadyExists();

        attributeRepository.Add(attribute);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateProductAttributeResponse(attribute.Id, attribute.Slug.Value);
    }
}