using RimWorld;
using Verse;

namespace GenderedMoralGuideTitles;

public static class MoralistRole
{
    public static bool IsMoralist(Precept? precept)
    {
        if (precept?.def == null || precept.def.leaderRole)
        {
            return false;
        }

        if (precept.def == PreceptDefOf.IdeoRole_Moralist)
        {
            return true;
        }

        return precept.def.roleTags != null && precept.def.roleTags.Contains("Moralist");
    }
}
