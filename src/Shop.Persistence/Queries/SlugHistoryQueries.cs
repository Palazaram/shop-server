using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Shop.Application.Catalog;
using Shop.Domain.Common;
using Shop.Domain.Errors;
using Shop.Domain.SlugHistory;

namespace Shop.Persistence.Queries;

internal sealed class SlugHistoryQueries(AppDbContext context) : ISlugHistoryQueries
{
    public async Task<Maybe<string>> FindCurrentSlugAsync(
        SlugOwnerType ownerType,
        string slug,
        CancellationToken cancellationToken)
    {
        Result<Slug, Error> slugResult = Slug.Create(slug);

        // Та же строгость, что и у живых адресов: искать перенаправление для строки,
        // которая слагом даже не является, незачем.
        if (slugResult.IsFailure
            || !string.Equals(slug, slugResult.Value.Value, StringComparison.Ordinal))
            return Maybe<string>.None;

        Slug parsedSlug = slugResult.Value;

        Guid ownerId = await context.SlugHistory
            .AsNoTracking()
            .Where(entry => entry.OwnerType == ownerType && entry.Slug == parsedSlug)
            .Select(entry => entry.OwnerId)
            .FirstOrDefaultAsync(cancellationToken);

        if (ownerId == Guid.Empty)
            return Maybe<string>.None;

        string? currentSlug = ownerType switch
        {
            SlugOwnerType.Category => await context.Categories
                .AsNoTracking()
                .Where(c => c.Id == ownerId)
                .Select(c => c.Slug.Value)
                .FirstOrDefaultAsync(cancellationToken),
            _ => await context.ProductVariants
                .AsNoTracking()
                .Where(v => v.Id == ownerId)
                .Select(v => v.Slug.Value)
                .FirstOrDefaultAsync(cancellationToken)
        };

        return currentSlug is null ? Maybe<string>.None : Maybe<string>.From(currentSlug);
    }
}
