using System.Globalization;
using System.Text;

namespace ProiectWeb.Services;

public static class TextNormalization
{
    public static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    public static bool ContainsNormalized(string source, string search)
    {
        if (string.IsNullOrWhiteSpace(search))
            return true;
        if (string.IsNullOrEmpty(source))
            return false;

        return RemoveDiacritics(source)
            .Contains(RemoveDiacritics(search), StringComparison.OrdinalIgnoreCase);
    }
}
