using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace GenderedMoralGuideTitles;

[HarmonyPatch(typeof(Precept_Role), nameof(Precept_Role.LabelForPawn))]
public static class Patch_LabelForPawn
{
    public static void Postfix(Precept_Role __instance, Pawn p, ref string __result)
    {
        if (p.gender != Gender.Female || !MoralistRole.IsMoralist(__instance))
        {
            return;
        }

        string female = MoralistTitleStore.GetFemale(__instance);
        if (!female.NullOrEmpty())
        {
            __result = female.CapitalizeFirst();
        }
    }
}

[HarmonyPatch(typeof(Precept_Role), nameof(Precept_Role.UIInfoSecondLine), MethodType.Getter)]
public static class Patch_UIInfoSecondLine
{
    public static void Postfix(Precept_Role __instance, ref string __result)
    {
        if (!MoralistRole.IsMoralist(__instance))
        {
            return;
        }

        string female = MoralistTitleStore.GetFemale(__instance);
        if (!female.NullOrEmpty() && female != __instance.Label)
        {
            __result += " (" + female.CapitalizeFirst() + ")";
        }
    }
}

[HarmonyPatch(typeof(Precept), nameof(Precept.GenerateNewName))]
public static class Patch_GenerateNewName
{
    public static void Postfix(Precept __instance, string __result)
    {
        if (!MoralistRole.IsMoralist(__instance))
        {
            return;
        }

        MoralistTitleStore.SetFemale(__instance, GenderedTitle.Feminize(__result));
    }
}

[HarmonyPatch(typeof(Precept), nameof(Precept.ExposeData))]
public static class Patch_ExposeData
{
    public static void Postfix(Precept __instance)
    {
        if (__instance is not Precept_Role || !MoralistRole.IsMoralist(__instance))
        {
            return;
        }

        string? female = MoralistTitleStore.GetFemaleRaw(__instance);
        Scribe_Values.Look(ref female, "moralistTitleFemale");
        if (Scribe.mode == LoadSaveMode.LoadingVars || Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            if (female.NullOrEmpty())
            {
                female = GenderedTitle.Feminize(__instance.Label);
            }

            MoralistTitleStore.SetFemale(__instance, female);
        }
        else if (female != null)
        {
            MoralistTitleStore.SetFemale(__instance, female);
        }
    }
}

[HarmonyPatch(typeof(Precept_Role), nameof(Precept_Role.CopyTo))]
public static class Patch_CopyTo
{
    public static void Postfix(Precept_Role __instance, Precept precept)
    {
        if (!MoralistRole.IsMoralist(__instance) || precept == null)
        {
            return;
        }

        MoralistTitleStore.SetFemale(precept, MoralistTitleStore.GetFemale(__instance));
    }
}

[HarmonyPatch(typeof(Dialog_EditPrecept), MethodType.Constructor, typeof(Precept))]
public static class Patch_DialogCtor
{
    public static void Postfix(Precept precept, ref string ___newPreceptNameFemale)
    {
        if (MoralistRole.IsMoralist(precept))
        {
            ___newPreceptNameFemale = MoralistTitleStore.GetFemale(precept);
        }
    }
}

[HarmonyPatch(typeof(Dialog_EditPrecept), "UpdateWindowHeight")]
public static class Patch_UpdateWindowHeight
{
    private static readonly MethodInfo SetInitialSizeAndPositionMethod =
        AccessTools.Method(typeof(Window), "SetInitialSizeAndPosition");

    public static void Postfix(Dialog_EditPrecept __instance, Precept ___precept, ref float ___windowHeight)
    {
        if (!MoralistRole.IsMoralist(___precept))
        {
            return;
        }

        ___windowHeight += 10f + 30f;
        SetInitialSizeAndPositionMethod.Invoke(__instance, null);
    }
}

[HarmonyPatch(typeof(Dialog_EditPrecept), "ApplyChanges")]
public static class Patch_ApplyChanges
{
    public static void Postfix(Precept ___precept, string ___newPreceptNameFemale, List<PreceptApparelRequirement> ___apparelRequirements)
    {
        if (!MoralistRole.IsMoralist(___precept))
        {
            return;
        }

        if (___apparelRequirements != null && ___apparelRequirements.Any(x => x.RequirementOverlapsOther(___apparelRequirements, out _)))
        {
            return;
        }

        MoralistTitleStore.SetFemale(___precept, ___newPreceptNameFemale);
        ___precept.ClearTipCache();
    }
}

[HarmonyPatch(typeof(Dialog_EditPrecept), nameof(Dialog_EditPrecept.DoWindowContents))]
public static class Patch_DoWindowContents
{
    private static readonly FieldInfo PreceptField = AccessTools.Field(typeof(Dialog_EditPrecept), "precept");
    private static readonly FieldInfo DefField = AccessTools.Field(typeof(Precept), nameof(Precept.def));
    private static readonly FieldInfo LeaderRoleField = AccessTools.Field(typeof(PreceptDef), nameof(PreceptDef.leaderRole));
    private static readonly FieldInfo FemaleNameField = AccessTools.Field(typeof(Dialog_EditPrecept), "newPreceptNameFemale");
    private static readonly MethodInfo GenerateNewNameMethod = AccessTools.Method(typeof(Precept), nameof(Precept.GenerateNewName));
    private static readonly MethodInfo ShowGenderedMethod = AccessTools.Method(typeof(Patch_DoWindowContents), nameof(ShowGenderedTitleFields));
    private static readonly MethodInfo TitleFieldKeyMethod = AccessTools.Method(typeof(Patch_DoWindowContents), nameof(TitleFieldKey));
    private static readonly MethodInfo ApplyRandomFemaleMethod = AccessTools.Method(typeof(Patch_DoWindowContents), nameof(ApplyMoralistRandomFemaleName));

    public static bool ShowGenderedTitleFields(Precept precept)
    {
        return precept.def.leaderRole || MoralistRole.IsMoralist(precept);
    }

    public static string TitleFieldKey(Precept precept)
    {
        return precept.def.leaderRole ? "LeaderTitle" : "Name";
    }

    public static void ApplyMoralistRandomFemaleName(Dialog_EditPrecept dialog)
    {
        Precept precept = (Precept)PreceptField.GetValue(dialog);
        if (MoralistRole.IsMoralist(precept))
        {
            FemaleNameField.SetValue(dialog, MoralistTitleStore.GetFemale(precept));
        }
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new(instructions);
        bool replacedLeaderCheck = false;
        int titleKeysReplaced = 0;
        bool patchedRandomize = false;

        for (int i = 0; i < codes.Count; i++)
        {
            if (!replacedLeaderCheck
                && i + 2 < codes.Count
                && codes[i].LoadsField(PreceptField)
                && codes[i + 1].LoadsField(DefField)
                && codes[i + 2].LoadsField(LeaderRoleField))
            {
                codes[i + 1] = new CodeInstruction(OpCodes.Call, ShowGenderedMethod);
                codes.RemoveAt(i + 2);
                replacedLeaderCheck = true;
            }

            if (titleKeysReplaced < 2
                && codes[i].opcode == OpCodes.Ldstr
                && codes[i].operand is string key
                && key == "LeaderTitle")
            {
                codes[i] = new CodeInstruction(OpCodes.Ldarg_0);
                codes.Insert(i + 1, new CodeInstruction(OpCodes.Ldfld, PreceptField));
                codes.Insert(i + 2, new CodeInstruction(OpCodes.Call, TitleFieldKeyMethod));
                titleKeysReplaced++;
                i += 2;
            }

            if (!patchedRandomize
                && codes[i].Calls(GenerateNewNameMethod))
            {
                int storeIndex = FindFemaleStore(codes, i);
                if (storeIndex >= 0)
                {
                    codes.Insert(storeIndex + 1, new CodeInstruction(OpCodes.Ldarg_0));
                    codes.Insert(storeIndex + 2, new CodeInstruction(OpCodes.Call, ApplyRandomFemaleMethod));
                    patchedRandomize = true;
                }
            }
        }

        if (!replacedLeaderCheck || titleKeysReplaced < 2 || !patchedRandomize)
        {
            Log.Error("[Gendered Moral Guide Titles] Failed to patch Dialog_EditPrecept.DoWindowContents (leader="
                + replacedLeaderCheck + ", titles=" + titleKeysReplaced + ", randomize=" + patchedRandomize + ").");
        }

        return codes;
    }

    private static int FindFemaleStore(List<CodeInstruction> codes, int generateCallIndex)
    {
        for (int i = generateCallIndex + 1; i < codes.Count && i < generateCallIndex + 8; i++)
        {
            if (codes[i].StoresField(FemaleNameField))
            {
                return i;
            }
        }

        return -1;
    }
}
