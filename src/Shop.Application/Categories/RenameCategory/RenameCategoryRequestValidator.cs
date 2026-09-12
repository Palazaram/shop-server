using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Categories;
using Shop.Domain.Errors;

namespace Shop.Application.Categories.RenameCategory;

public sealed class RenameCategoryRequestValidator : AbstractValidator<RenameCategoryRequest>
{
    public RenameCategoryRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Name)
            .NotEmpty()
                .WithError(DomainErrors.Categories.NameIsRequired())
            .MaximumLength(Category.MaxNameLength)
                .WithError(DomainErrors.Categories.NameTooLong(Category.MaxNameLength));
    }
}