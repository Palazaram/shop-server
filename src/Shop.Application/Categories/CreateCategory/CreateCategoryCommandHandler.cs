using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Categories;
using Shop.Domain.Common;
using Shop.Domain.Errors;

namespace Shop.Application.Categories.CreateCategory;

internal sealed class CreateCategoryCommandHandler(
    ICategoryRepository categoryRepository,
    ISlugGenerator slugGenerator,
    IUnitOfWork unitOfWork) 
        : ICommandHandler<CreateCategoryCommand, CreateCategoryResponse>
{
    public async Task<Result<CreateCategoryResponse, Error>> HandleAsync(
        CreateCategoryCommand command,
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
                : DomainErrors.Categories.SlugCannotBeGenerated();

        Result<Category, Error> categoryResult =
            Category.Create(command.Name, slugResult.Value, command.ParentId);
        if (categoryResult.IsFailure)
            return categoryResult.Error;

        Category category = categoryResult.Value;

        if (category.ParentId.HasValue)
        {
            Maybe<Category> parent =
                await categoryRepository.GetByIdAsync(category.ParentId.Value, cancellationToken);

            if (parent.HasNoValue)
                return DomainErrors.Categories.ParentNotFound();

            if (parent.Value.ParentId.HasValue)
                return DomainErrors.Categories.MaxDepthExceeded();
        }

        if (await categoryRepository.ExistsByNameAsync(
                category.Name, category.ParentId, null, cancellationToken))
            return DomainErrors.Categories.NameAlreadyExists();

        if (await categoryRepository.ExistsBySlugAsync(
                category.Slug, category.ParentId, cancellationToken))
            return DomainErrors.Categories.SlugAlreadyExists();

        categoryRepository.Add(category);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateCategoryResponse(category.Id, category.Slug.Value);
    }
}