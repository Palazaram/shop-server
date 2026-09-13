using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Categories;
using Shop.Domain.Errors;
using Shop.Domain.Manufacturers;
using Shop.Domain.Products;

namespace Shop.Application.Products.UpdateProduct;

internal sealed class UpdateProductCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IManufacturerRepository manufacturerRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<UpdateProductCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        Maybe<Product> maybeProduct =
            await productRepository.GetByIdAsync(command.ProductId, cancellationToken);

        if (maybeProduct.HasNoValue)
            return DomainErrors.Products.NotFound();

        Product product = maybeProduct.Value;

        Guid categoryId = command.CategoryId!.Value;
        Guid manufacturerId = command.ManufacturerId!.Value;

        if (!await categoryRepository.ExistsAsync(categoryId, cancellationToken))
            return DomainErrors.Products.CategoryNotFound();

        if (await categoryRepository.HasChildrenAsync(categoryId, cancellationToken))
            return DomainErrors.Products.CategoryIsNotLeaf();

        if (!await manufacturerRepository.ExistsAsync(manufacturerId, cancellationToken))
            return DomainErrors.Products.ManufacturerNotFound();

        UnitResult<Error> renameResult = product.Rename(command.Name);
        if (renameResult.IsFailure)
            return renameResult;

        UnitResult<Error> descriptionResult = product.ChangeDescription(command.Description);
        if (descriptionResult.IsFailure)
            return descriptionResult;

        product.MoveToCategory(categoryId);
        product.ChangeManufacturer(manufacturerId);

        if (await productRepository.ExistsByNameAsync(
                product.Name, product.ManufacturerId, product.Id, cancellationToken))
            return DomainErrors.Products.NameAlreadyExists();

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}