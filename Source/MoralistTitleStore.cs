using System.Runtime.CompilerServices;
using RimWorld;
using Verse;

namespace GenderedMoralGuideTitles;

public static class MoralistTitleStore
{
    private sealed class Data
    {
        public string? Female;
    }

    private static readonly ConditionalWeakTable<Precept, Data> Table = new();

    public static string GetFemale(Precept precept)
    {
        if (Table.TryGetValue(precept, out Data? data) && !data.Female.NullOrEmpty())
        {
            return data.Female!;
        }

        return GenderedTitle.Feminize(precept.Label);
    }

    public static string? GetFemaleRaw(Precept precept)
    {
        if (Table.TryGetValue(precept, out Data? data))
        {
            return data.Female;
        }

        return null;
    }

    public static void SetFemale(Precept precept, string? female)
    {
        Table.GetValue(precept, _ => new Data()).Female = female;
    }
}
