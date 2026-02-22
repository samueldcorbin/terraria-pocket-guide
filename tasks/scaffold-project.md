# Scaffold tModLoader Project

## Goal
Create the minimal tModLoader mod skeleton that builds and loads in-game.

## Files to create
- `build.txt` — displayName, author, version, includePDB=true (dev)
- `GuideVoodooDollMod.csproj` — minimal, imports tModLoader.targets
- `GuideVoodooDollMod.cs` — empty Mod subclass
- `description.txt` — short mod description
- `.gitignore` — bin/, obj/, *.tmod, .vs/
- `Properties/launchSettings.json` — debug launch config

## Internal mod name
`GuideVoodooDollMod` (or shorter if preferred — ask user)

## Notes
- Do NOT set TargetFramework manually; tModLoader.targets handles it
- LangVersion should be `latest`
- AssemblyName must match mod folder name
