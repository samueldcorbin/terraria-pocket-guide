# Hook into item hover to show Guide recipe lookup when doll is active

## Status: Complete

## What was built
- `PocketGuideSystem` (ModSystem) — hooks `PostDrawInterface` to detect item hover
  - Checks `DollActive && DollPresent` and `Main.HoverItem` is non-air
  - Caches by `_lastHoverItemType` to avoid re-scanning every frame
  - `CollectRecipesUsing(itemType)` replicates `Recipe.CollectGuideRecipes` logic
  - `HoverRecipes` (static `List<Recipe>`) exposes matched recipes for the UI task

## Recipe matching logic
- Direct type match: `req.type == itemType`
- Group match: `recipe.AcceptedByItemGroups(itemType, req.type)` — covers all recipe groups including legacy anyWood/anySand/etc. (legacy flags are converted to `acceptedGroups` entries by `ReplaceItemUseFlagWithGroup` during recipe setup, then cleared)

## Key finding from decompilation
- `CollectGuideRecipes` (Recipe.cs:678) is private and checks both `AcceptedByItemGroups` AND legacy `useWood`/`useSand`/etc. private methods
- But `ReplaceItemUseFlagWithGroup` (Recipe.cs:14589) converts legacy flags to `acceptedGroups` entries and sets `flag = false`, so at runtime the legacy methods always return false
- Therefore `AcceptedByItemGroups` alone is sufficient for group matching
