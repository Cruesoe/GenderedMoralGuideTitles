using System.Collections.Generic;
using Verse;

namespace GenderedMoralGuideTitles;

public class GenderedTitleDef : Def
{
    [NoTranslate]
    public string male = string.Empty;

    [NoTranslate]
    public string female = string.Empty;

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (string error in base.ConfigErrors())
        {
            yield return error;
        }

        if (male.NullOrEmpty())
        {
            yield return "male is empty";
        }

        if (female.NullOrEmpty())
        {
            yield return "female is empty";
        }
    }
}
