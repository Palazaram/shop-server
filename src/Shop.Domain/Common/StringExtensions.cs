using System.Text.RegularExpressions;

namespace Shop.Domain.Common;

public static partial class StringExtensions
{
    public static string CollapseWhitespace(this string value)
        => WhitespaceRegex().Replace(value.Trim(), " ");

    [GeneratedRegex(@"\s+")]
    private static partial Regex WhitespaceRegex();
}