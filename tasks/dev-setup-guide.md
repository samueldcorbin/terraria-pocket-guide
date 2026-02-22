# Dev Environment Setup Guide

## Goal
Walk the user through setting up their VSCode + tModLoader dev environment for fast iteration, including hot reload.

## Steps to cover
1. **VSCode extensions** — install C# Dev Kit (which includes C# base extension)
2. **Enable hot reload** — set `csharp.experimental.debug.hotReload: true` in VSCode settings
3. **tModLoader mod source location** — symlink or place project in `ModSources/` so tModLoader finds it
4. **Launch configuration** — set up `.vscode/launch.json` to launch tModLoader with debugger attached
5. **`-skipselect` setup** — create/edit `cli-argsConfig.txt` in tModLoader install dir with test player/world names
6. **Test the loop** — build + reload from in-game, verify hot reload works (edit a `Main.NewText()` string, Ctrl+Shift+Enter, see it change without reload)
7. **Troubleshooting** — common issues (wrong .NET SDK version, mod not showing up, hot reload not applying)

## Notes
- This should be a walkthrough we give to the user, not a file we ship with the mod
- User is on Linux (WSL2) — make sure paths and instructions are Linux-appropriate
- tModLoader on Linux may be Steam or manual install — cover both
