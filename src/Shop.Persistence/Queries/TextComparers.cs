using System.Globalization;

namespace Shop.Persistence.Queries;

internal static class TextComparers
{
    public static readonly StringComparer Ukrainian =
        StringComparer.Create(CultureInfo.GetCultureInfo("uk-UA"), ignoreCase: false);
}