using System.Text;
using Shop.Application.Abstractions;

namespace Shop.Infrastructure;

public sealed class SlugGenerator : ISlugGenerator
{
    // Таблица КМУ №55 — та же транслитерация, что в загранпаспортах и на указателях.
    // Слаг попадает в адрес страницы, и адрес украинского магазина должен читаться
    // по-украински: «Гербіциди» → herbitsydy, а не gerbicidi.
    private static readonly Dictionary<char, string> Transliteration = new()
    {
        ['а'] = "a",
        ['б'] = "b",
        ['в'] = "v",
        ['г'] = "h",
        ['ґ'] = "g",
        ['д'] = "d",
        ['е'] = "e",
        ['є'] = "ie",
        ['ж'] = "zh",
        ['з'] = "z",
        ['и'] = "y",
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
        ['ц'] = "ts",
        ['ч'] = "ch",
        ['ш'] = "sh",
        ['щ'] = "shch",
        ['ь'] = "",
        ['ю'] = "iu",
        ['я'] = "ia",

        // Апостроф во всех начертаниях по стандарту исчезает.
        ['\''] = "",
        ['\u2019'] = "",
        ['\u02bc'] = "",

        // русские буквы, которых нет в украинском алфавите
        ['ё'] = "e",
        ['ъ'] = "",
        ['ы'] = "y",
        ['э'] = "e",
    };

    // Пять букв в начале слова пишутся иначе: Yulia, а не Iuliia.
    private static readonly Dictionary<char, string> WordInitial = new()
    {
        ['є'] = "ye",
        ['ї'] = "yi",
        ['й'] = "y",
        ['ю'] = "yu",
        ['я'] = "ya",
    };

    public string Generate(string? source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return string.Empty;

        StringBuilder builder = new(source.Length);
        bool wordStart = true;
        char previous = '\0';

        foreach (char symbol in source.ToLowerInvariant())
        {
            if (symbol == 'г' && previous == 'з')
            {
                // «зг» передаётся как zgh: иначе его не отличить от «ж».
                builder.Append("gh");
            }
            else if (wordStart && WordInitial.TryGetValue(symbol, out string? initial))
            {
                builder.Append(initial);
            }
            else if (Transliteration.TryGetValue(symbol, out string? latin))
            {
                // Мягкий знак и апостроф исчезают и слова не начинают,
                // поэтому позицию в слове не сдвигают.
                if (latin.Length == 0)
                {
                    previous = symbol;
                    continue;
                }

                builder.Append(latin);
            }
            else if (char.IsAsciiLetterOrDigit(symbol))
            {
                builder.Append(symbol);
            }
            else
            {
                if (builder.Length > 0 && builder[^1] != '-')
                    builder.Append('-');

                wordStart = true;
                previous = '\0';
                continue;
            }

            wordStart = false;
            previous = symbol;
        }

        return builder.ToString().Trim('-');
    }
}