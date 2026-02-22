# Finish Rename to PocketGuide

## What happened
The repo was renamed from `terraria-guide-doll` to `terraria-pocket-guide`. New `PocketGuide.cs` and `PocketGuide.csproj` were created but landed in a stale `/root/terraria-guide-doll/` dir. They need to be recreated in this repo.

## Remaining steps
1. Create `PocketGuide.cs` — empty Mod subclass, namespace `PocketGuide`
2. Create `PocketGuide.csproj` — same as old one but AssemblyName/RootNamespace = `PocketGuide`
3. `git rm GuideVoodooDollMod.cs GuideVoodooDollMod.csproj`
4. Update `build.txt`: change `displayName` to `Pocket Guide`
5. Commit everything
