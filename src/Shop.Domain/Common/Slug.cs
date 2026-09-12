using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Shop.Domain.Errors;

namespace Shop.Domain.Common;

public sealed partial class Slug : SimpleValueObject<string>
{
    public const int MaxLength = 200;

    private Slug(string value) : base(value) { }

    public static Result<Slug, Error> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return DomainErrors.Slugs.IsRequired();

        string trimmed = value.Trim();

        if (trimmed.Length > MaxLength)
            return DomainErrors.Slugs.TooLong(MaxLength);

        if (!SlugRegex().IsMatch(trimmed))
            return DomainErrors.Slugs.InvalidFormat();

        return new Slug(trimmed);
    }

    [GeneratedRegex(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();
}