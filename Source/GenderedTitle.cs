using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Verse;

namespace GenderedMoralGuideTitles;

public static class GenderedTitle
{
    private static List<(Regex Pattern, string Female)>? compiled;

    public static string Feminize(string? title)
    {
        if (title.NullOrEmpty())
        {
            return string.Empty;
        }

        EnsureCompiled();
        string result = title!;
        foreach ((Regex pattern, string female) in compiled!)
        {
            result = pattern.Replace(result, match => MatchCase(match.Value, female));
        }

        return result;
    }

    private static void EnsureCompiled()
    {
        if (compiled != null)
        {
            return;
        }

        compiled = DefDatabase<GenderedTitleDef>.AllDefsListForReading
            .Where(def => !def.male.NullOrEmpty() && !def.female.NullOrEmpty() && def.male != def.female)
            .OrderByDescending(def => def.male.Length)
            .Select(def => (
                new Regex(@"\b" + Regex.Escape(def.male) + @"\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant),
                def.female))
            .ToList();
    }

    private static string MatchCase(string original, string replacement)
    {
        if (original.Length == 0)
        {
            return replacement;
        }

        if (original.All(char.IsUpper))
        {
            return replacement.ToUpperInvariant();
        }

        if (char.IsUpper(original[0]))
        {
            return replacement.CapitalizeFirst();
        }

        return replacement;
    }
}
