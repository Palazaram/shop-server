using FluentValidation;
using Shop.Application.Extensions;
using Shop.Domain.Errors;

namespace Shop.Application.Products.ReorderProductImages;

public sealed class ReorderProductImagesRequestValidator
    : AbstractValidator<ReorderProductImagesRequest>
{
    public ReorderProductImagesRequestValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Continue;
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.ImageIds)
            .NotEmpty()
                .WithError(DomainErrors.Products.ImageOrderIsRequired())
            .Must(ids => ids!.All(id => id != Guid.Empty))
                .WithError(DomainErrors.Products.ImageIdIsInvalid())
            .Must(ids => ids!.Distinct().Count() == ids!.Count)
                .WithError(DomainErrors.Products.DuplicateImageInOrder());
    }
}