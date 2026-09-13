using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Categories;
using Shop.Domain.Errors;
using Shop.Domain.Manufacturers;
using Shop.Domain.Products;

namespace Shop.Application.Products.CreateProduct;

internal sealed class CreateProductCommandHandler(
    IProductRepository productRepository,
    ICategoryRepository categoryRepository,
    IManufacturerRepository manufacturerRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<CreateProductCommand, CreateProductResponse>
{
    public async Task<Result<CreateProductResponse, Error>> HandleAsync(CreateProductCommand command, CancellationToken cancellationToken)
    {
        Guid categoryId = command.CategoryId!.Value;
        Guid manufacturerId = command.ManufacturerId!.Value;

        if (!await categoryRepository.ExistsAsync(categoryId, cancellationToken))
            return DomainErrors.Products.CategoryNotFound();

        if (await categoryRepository.HasChildrenAsync(categoryId, cancellationToken))
            return DomainErrors.Products.CategoryIsNotLeaf();

        if (!await manufacturerRepository.ExistsAsync(manufacturerId, cancellationToken))
            return DomainErrors.Products.ManufacturerNotFound();

        Result<Product, Error> productResult = Product.Create(
            name: command.Name,
            description: command.Description,
            categoryId: categoryId,
            manufacturerId: manufacturerId);

        if (productResult.IsFailure)
            return productResult.Error;

        Product product = productResult.Value;

        if (await productRepository.ExistsByNameAsync(
                product.Name, product.ManufacturerId, null, cancellationToken))
            return DomainErrors.Products.NameAlreadyExists();

        productRepository.Add(product);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateProductResponse(product.Id);
    }
}