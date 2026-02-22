# tModLoader Dev Workflow Research

## Project Structure
- Mod lives in `ModSources/<ModName>/`
- Required: `build.txt`, `<ModName>.cs` (Mod subclass), `<ModName>.csproj`
- Optional: `description.txt`, `icon.png` (64x64), localization `.hjson`, sounds `.ogg`
- .csproj is minimal — imports `tModLoader.targets` which handles framework refs
- Targets .NET 8 / C# 12 (as of tModLoader v2024.03+)

## Build/Reload Cycle
- **In-game**: Workshop > Develop Mods > "Build + Reload" — compiles .cs, packages .tmod, reloads
- **Visual Studio**: F5 builds + launches with debugger attached
- **No true hot-reload** — "Build + Reload" always kicks to main menu
- **Hot Reload via VSCode C# Dev Kit** — set `csharp.experimental.debug.hotReload: true`, apply changes with Ctrl+Shift+Enter. Requires .NET 8+ on Linux (which tModLoader targets). Same EnC restrictions (can't add types/change signatures) but great for logic tweaks and UI iteration.
- **Edit and Continue** (`-eac` flag, Windows+VS only) — equivalent feature in full Visual Studio

## Developer Tools
- `Mod.Logger.Info/Warn/Error()` — log4net, writes to client.log
- `Main.NewText("msg")` — in-game chat debug messages
- `CombatText.NewText()` — floating world text
- `Dust.QuickDust/QuickBox/QuickDustLine()` — geometry visualization

## Fast Iteration Tips
- `-skipselect "PlayerName:WorldName"` in `cli-argsConfig.txt` — auto-select char/world
- `includePDB = true` in build.txt — line numbers in stack traces
- Windowed mode for easy alt-tab
- "Debug Tools" mod on Workshop for in-game cheats/inspection

## How Mods Load
- tModLoader compiles .cs via MSBuild/Roslyn → packages into .tmod (custom binary)
- Loading: reads .tmod → extracts DLL → loads assembly → instantiates Mod class → lifecycle hooks
- `noCompile = true` in build.txt skips compilation (expects pre-built DLL in lib/)
