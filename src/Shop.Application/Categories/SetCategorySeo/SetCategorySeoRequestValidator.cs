using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Errors;
using Shop.Domain.Categories;

namespace Shop.Application.Categories.SetCategorySeo;

public sealed class SetCategorySeoRequestValidator : AbstractValidator<SetCategorySeoRequest>
{
    public SetCategorySeoRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        // Оба поля необязательны: null и пустая строка означают «не заполнено».
        RuleFor(x => x.MetaTitle)
            .MaximumLength(Category.MaxMetaTitleLength)
                .WithError(DomainErrors.Categories.MetaTitleTooLong(
                    Category.MaxMetaTitleLength));

        RuleFor(x => x.MetaDescription)
            .MaximumLength(Category.MaxMetaDescriptionLength)
                .WithError(DomainErrors.Categories.MetaDescriptionTooLong(
                    Category.MaxMetaDescriptionLength));
    }
}
