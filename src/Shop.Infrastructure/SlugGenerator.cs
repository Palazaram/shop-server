using System.Text;
using Shop.Application.Abstractions;

namespace Shop.Infrastructure;

public sealed class SlugGenerator : ISlugGenerator
{
    private static readonly Dictionary<char, string> Transliteration = new()
    {
        ['а'] = "a",
        ['б'] = "b",
        ['в'] = "v",
        ['г'] = "g",
        ['ґ'] = "g",
        ['д'] = "d",
        ['е'] = "e",
        ['є'] = "ie",
        ['ж'] = "zh",
        ['з'] = "z",
        ['и'] = "i",
        ['і'] = "i",
        ['ї'] = "i",
        ['й'] = "i",
        ['к'] = "k",
        ['л'] = "l",
        ['м'] = "m",
        ['н'] = "n",
        ['о'] = "o",
        ['п'] = "p",
        ['р'] = "r",
        ['с'] = "s",
        ['т'] = "t",
        ['у'] = "u",
        ['ф'] = "f",
        ['х'] = "kh",
        ['ц'] = "c",
        ['ч'] = "ch",
        ['ш'] = "sh",
        ['щ'] = "shch",
        ['ь'] = "",
        ['ю'] = "iu",
        ['я'] = "ia",

        // русские буквы, которых нет в украинском алфавите
        ['ё'] = "e",
        ['ъ'] = "",
        ['ы'] = "y",
        ['э'] = "e",
    };

    public string Generate(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return string.Empty;

        StringBuilder builder = new(source.Length);

        foreach (char symbol in source.ToLowerInvariant())
        {
            if (Transliteration.TryGetValue(symbol, out string? latin))
                builder.Append(latin);
            else if (char.IsAsciiLetterOrDigit(symbol))
                builder.Append(symbol);
            else if (builder.Length > 0 && builder[^1] != '-')
                builder.Append('-');
        }

        return builder.ToString().Trim('-');
    }
}