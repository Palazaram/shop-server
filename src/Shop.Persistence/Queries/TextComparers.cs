using System.Globalization;

namespace Shop.Persistence.Queries;

internal static class TextComparers
{
    public static readonly StringComparer Ukrainian =
        StringComparer.Create(CultureInfo.GetCultureInfo("uk-UA"), ignoreCase: false);

    /// <summary>
    /// Тот же порядок, но на стороне базы. Нужен там, где сортировать в памяти нельзя,
    /// то есть везде, где есть пагинация.
    /// </summary>
    public const string UkrainianCollation = "uk-UA-x-icu";
}