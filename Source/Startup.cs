using HarmonyLib;
using Verse;

namespace GenderedMoralGuideTitles;

[StaticConstructorOnStartup]
public static class Startup
{
    static Startup()
    {
        new Harmony("cruesoe.genderedmoralguidetitles").PatchAll();
    }
}
