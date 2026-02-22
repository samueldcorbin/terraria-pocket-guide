# Recipe tooltip UI overlay

## Status: In Progress

## What's done
- Debug text rendering in `PocketGuideSystem.DrawRecipeList` — draws recipe strings at (20, 80) in top-left corner
- Format: `Result (qty) = Ingredient1 (qty) + Ingredient2 (qty) @ Station`
- Uses `ChatManager.DrawColorCodedStringWithShadow` and `FontAssets.MouseText`
- Crafting station names via `Lang.GetMapObjectName(MapHelper.TileToLookup(tileId, 0))`

## What's remaining
- Replace debug text with proper hover tooltip UI
- Design should fit a hover context (not replicate Guide NPC panel layout)
- Consider: positioning relative to cursor/item, background panel, item icons, scrolling for long lists
- The Guide NPC's recipe panel is the *content* reference, not the *layout* reference (per CLAUDE.md)

## Key architecture
- `PocketGuideSystem.HoverRecipes` (static `List<Recipe>`) holds matched recipes
- `PostDrawInterface` is the draw hook (has SpriteBatch)
- Recipe collection is cached by `_lastHoverItemType` to avoid re-scanning every frame
