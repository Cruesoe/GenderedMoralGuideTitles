# Gendered Moral Guide Titles

RimWorld 1.6 Ideology mod. The Moral Guide role gets **male and female titles**, matching how Leader already works.

- New ideoligions generate a female form when the noun has one (`priest` / `priestess`, `abbot` / `abbess`, `monk` / `nun`, and similar).
- Assigned colonists show the title that matches their gender.
- The ideoligion editor has separate male and female name fields, plus Randomize.

Existing saves keep the current Moral Guide name as the male title and fill in a female form automatically. Edit or randomize to change either one.

Requires [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077) and **Ideology**.

## Install

Copy this folder to `RimWorld\Mods\`, or add it as a local mod in RimSort.

## Build

```
dotnet build Source\GenderedMoralGuideTitles.csproj -c Debug
```

The DLL is copied to `1.6\Assemblies\GenderedMoralGuideTitles.dll` and to `RimWorld\Mods\Gendered Moral Guide Titles\1.6\Assemblies\` if that folder exists.
