using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Errors;

namespace Shop.Application.Categories.ReorderCategories;

public sealed class ReorderCategoriesCommandValidator : AbstractValidator<ReorderCategoriesCommand>
{
    public ReorderCategoriesCommandValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ParentId)
            .Must(parentId => parentId != Guid.Empty)
                .WithError(DomainErrors.Categories.ParentIdIsInvalid())
            .When(x => x.ParentId.HasValue);

        RuleFor(x => x.CategoryIds)
            .NotEmpty()
                .WithError(DomainErrors.Categories.OrderIsRequired())
            .Must(ids => ids!.All(id => id != Guid.Empty))
                .WithError(DomainErrors.Categories.OrderIdIsInvalid())
            .Must(ids => ids!.Distinct().Count() == ids!.Count)
                .WithError(DomainErrors.Categories.DuplicateCategoryInOrder());
    }
}