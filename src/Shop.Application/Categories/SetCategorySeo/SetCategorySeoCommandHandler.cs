using CSharpFunctionalExtensions;
using Shop.Application.Abstractions;
using Shop.Domain.Abstractions;
using Shop.Domain.Categories;
using Shop.Domain.Errors;

namespace Shop.Application.Categories.SetCategorySeo;

internal sealed class SetCategorySeoCommandHandler(
    ICategoryRepository categoryRepository,
    IUnitOfWork unitOfWork)
        : ICommandHandler<SetCategorySeoCommand>
{
    public async Task<UnitResult<Error>> HandleAsync(
        SetCategorySeoCommand command,
        CancellationToken cancellationToken)
    {
        Maybe<Category> maybeCategory =
            await categoryRepository.GetByIdAsync(command.CategoryId, cancellationToken);

        if (maybeCategory.HasNoValue)
            return DomainErrors.Categories.NotFound();

        UnitResult<Error> result =
            maybeCategory.Value.SetSeo(command.MetaTitle, command.MetaDescription);

        if (result.IsFailure)
            return result;

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}
