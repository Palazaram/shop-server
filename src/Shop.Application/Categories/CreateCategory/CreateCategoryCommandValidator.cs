using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Categories;
using Shop.Domain.Common;
using Shop.Domain.Errors;

namespace Shop.Application.Categories.CreateCategory;

public sealed class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
{
    public CreateCategoryCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty()
                .WithError(DomainErrors.Categories.NameIsRequired())
            .MaximumLength(Category.MaxNameLength)
                .WithError(DomainErrors.Categories.NameTooLong(Category.MaxNameLength));

        RuleFor(x => x.Slug)
            .MustBeValueObject(Slug.Create)
            .When(x => !string.IsNullOrWhiteSpace(x.Slug));

        RuleFor(x => x.ParentId)
            .Must(parentId => parentId != Guid.Empty)
                .WithError(DomainErrors.Categories.ParentIdIsInvalid())
            .When(x => x.ParentId.HasValue);
    }
}