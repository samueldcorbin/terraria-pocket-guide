# Vanilla Guide Crafting UI Internals

## Research Summary

Detailed decompilation analysis of how Terraria's Guide NPC crafting UI works internally.
Source: decompiled `Terraria.Main`, `Terraria.Recipe`, `Terraria.UI.ItemSlot`.

---

## 1. How `Main.InGuideCraftMenu` Controls the Guide Recipe Panel

**Declaration** (Main.cs:2315):
```csharp
public static bool InGuideCraftMenu;
```

**Set to true** when player clicks "Crafting" button while talking to Guide NPC (type 22) (Main.cs:52486-52493):
```csharp
else if (npc[player[myPlayer].talkNPC].type == 22)
{
    playerInventory = true;
    npcChatText = "";
    SoundEngine.PlaySound(12);
    InGuideCraftMenu = true;
    UILinkPointNavigator.GoToDefaultPage();
}
```

**Prerequisites to stay true** (Main.cs:54261): Must have no chest open, no shop open, must still be talking to an NPC (`talkNPC != -1`), and not in reforge menu:
```csharp
if (player[myPlayer].chest != -1 || npcShop != 0 || player[myPlayer].talkNPC == -1 || InReforgeMenu)
{
    InGuideCraftMenu = false;
    player[myPlayer].dropItemCheck();
    Recipe.FindRecipes();
}
```

**Set to false** in several places:
- When starting to talk to a different NPC (Main.cs:56170)
- When inventory closes (DrawInterface_26_InterfaceLogic3, Main.cs:59338)
- When prerequisites fail (Main.cs:54263)
- Mutually exclusive with InReforgeMenu (Main.cs:54130-54135)

**CRITICAL**: `talkNPC == -1` closes the guide menu. This is the main obstacle for our mod -- we need to either keep a fake talkNPC, bypass this check, or use an entirely different rendering path.

---

## 2. How `Main.guideItem` Is Used

**Declaration** (Main.cs:2149):
```csharp
public static Item guideItem = new Item();
```

### Setting guideItem
- **Direct click**: Player clicks the guide material slot with a material item on cursor. Handled by `ItemSlot.LeftClick` with context 7. `PickItemMovementAction` (ItemSlot.cs:1209-1213) only allows `checkItem.material || checkItem.type == 0`.
- **Shift-click**: When `Main.cursorOverride == 9` and `Main.InGuideCraftMenu`, does `Utils.Swap(ref inv[slot], ref Main.guideItem)` (ItemSlot.cs:653-657).

### Reading guideItem
- **Recipe filtering** (Recipe.cs:494): `if (!Main.guideItem.IsAir && Main.guideItem.Name != "")` triggers `CollectGuideRecipes()` instead of normal recipe collection.
- **Text display** (Main.cs:55070-55076): Shows "Place a material here" when air, or "Showing recipes that use [Name]" when populated.
- **Craft blocking** (Main.cs:55247): `if (focusRecipe == recipeIndex && guideItem.IsAir)` -- crafting is only allowed when guideItem is empty (normal crafting mode). When guideItem has an item, clicking a recipe just focuses it (browse-only).

### Dropping guideItem on close
`Player.dropItemCheck()` (Player.cs:5833-5847): When `!Main.InGuideCraftMenu && Main.guideItem.type > 0`, returns guideItem to player inventory (or drops it on the ground if inventory is full), then sets `Main.guideItem = new Item()`.

---

## 3. How the Material Slot Is Drawn

Located in `DrawInventory()` (Main.cs:54259-54285):

```csharp
else if (InGuideCraftMenu)
{
    // Close guard (talkNPC, chest, shop checks)
    ...
    // Position and text
    DrawGuideCraftText(num54, val9, out var inventoryX, out var inventoryY);

    // Hit test for mouse interaction
    if (mouseX >= inventoryX && mouseX <= inventoryX + InventoryBack.Width * inventoryScale
        && mouseY >= inventoryY && mouseY <= inventoryY + InventoryBack.Height * inventoryScale
        && !PlayerInput.IgnoreMouseInterface)
    {
        player[myPlayer].mouseInterface = true;
        craftingHide = true;
        ItemSlot.OverrideHover(ref guideItem, 7);   // Context 7 = guide slot
        ItemSlot.LeftClick(ref guideItem, 7);
        if (mouseLeftRelease && mouseLeft)
            Recipe.FindRecipes();
        ItemSlot.RightClick(ref guideItem, 7);
        ItemSlot.MouseHover(ref guideItem, 7);
    }
    ItemSlot.Draw(spriteBatch, ref guideItem, 7, new Vector2(inventoryX, inventoryY));
}
```

**Position**: `inventoryX = 73`, `inventoryY = 331 + adjY` where `adjY = (screenHeight - 600) / 2` (the vertical centering offset).

**Context 7** is the "guide slot" context throughout `ItemSlot`:
- `PickItemMovementAction`: Only accepts `material` items or air
- `OverrideHover`: Context 7 with shift shows cursor override 7 (take-to-inventory arrow)
- Draw: Uses standard `ItemSlot.Draw` with context 7

---

## 4. The "Showing items that use __" Text

Rendered in `DrawGuideCraftText()` (Main.cs:55051-55132):

### Localization keys
- `Lang.inter[24]` = `LegacyInterface.24` = **"Place a material here"** (shown when guideItem is air)
- `Lang.inter[21]` = `LegacyInterface.21` = **"Showing recipes that use"** (followed by `guideItem.Name`)
- `Lang.inter[22]` = `LegacyInterface.22` = **"Required objects:"** (crafting station label)
- `Lang.inter[23]` = `LegacyInterface.23` = **"None"** (when no crafting station needed)
- `Lang.inter[25]` = `LegacyInterface.25` = **"Crafting"** (header above recipe list in normal mode)

### Text positioning
```csharp
// Main text position
Vector2 val2 = new Vector2(inventoryX + 50, inventoryY + 12);
// If required objects text exists, shift main text up 14px
val2.Y -= 14f;
// Required objects line drawn 26px below
Vector2 val3 = val2 + new Vector2(0f, 26f);
```

### Required objects text
When guideItem is populated and a recipe is focused, the method builds a comma-separated list of:
- Required tiles (via `Lang.GetMapObjectName`)
- Water/Honey/Lava/Snow/Graveyard biome requirements
- Custom `recipe.Conditions`
- Falls back to "None" if no requirements

---

## 5. How the Recipe List Panel Renders

### Small recipe list (vertical strip on left side)
Main.cs:54298-54390 — Loop over `numAvailableRecipes`:

```csharp
for (int num66 = 0; num66 < Recipe.maxRecipes; num66++)
{
    // Scale based on distance from focused recipe (smooth scrolling)
    inventoryScale = 100f / (Math.Abs(availableRecipeY[num66]) + 100f);
    // Clamp minimum scale
    if (inventoryScale < 0.75) inventoryScale = 0.75f;

    // Position: x=46-26*scale, y=410+scrollOffset*scale-30*scale+adjY
    int num67 = (int)(46f - 26f * inventoryScale);
    int num68 = (int)(410f + availableRecipeY[num66] * inventoryScale - 30f * inventoryScale + num54);

    // Fade alpha based on distance
    // Hit test + HoverOverCraftingItemButton(num66)
    // Draw with ItemSlot.Draw context 22
    ItemSlot.Draw(spriteBatch, ref recipe[availableRecipe[num66]].createItem, 22, ...);
}
```

### Focused recipe ingredients
Main.cs:54391-54448 — Below the recipe list, shows ingredients of focused recipe:

```csharp
for (int num71 = 0; num71 < recipe[availableRecipe[focusRecipe]].requiredItem.Count; num71++)
{
    int num72 = 80 + num71 * 40;   // x position (horizontal row)
    int num73 = 380 + num54;       // y position
    inventoryScale = 0.6f;
    // Hit test -> SetRecipeMaterialDisplayName(num71)
    // Draw with ItemSlot.Draw context 22
    ItemSlot.Draw(spriteBatch, ref inv2, 22, new Vector2(num72, num73));
}
```

### Big recipe list (expanded grid)
Main.cs:54486-54609 — Toggled by `recBigList`:

```csharp
int num79 = 42;  // cell size
int num80 = 340; // top offset
int num81 = 310; // left offset
int num82 = (screenWidth - num81 - 280) / num79;  // columns
int num83 = (screenHeight - num80 - 20) / num79;  // rows
```

### Guide-specific layout adjustment
Main.cs:54458-54461:
```csharp
if (InGuideCraftMenu)
{
    num77 -= 150;  // Move the expand/collapse toggle button 150px up
}
```

### Crafting vs Browse mode
`HoverOverCraftingItemButton` (Main.cs:55241-55292): When `guideItem.IsAir`, clicking a focused recipe actually crafts it. When guideItem is populated (Guide browse mode), clicking only changes focus — no crafting occurs.

---

## 6. How `playerInventory` and Container/Chest State Interact

### Opening the Guide menu requires:
1. `playerInventory = true` (set when clicking "Crafting" in Guide dialogue)
2. `InGuideCraftMenu = true`
3. `talkNPC` must point to a valid NPC (the Guide)
4. `npcChatText = ""` (clears the chat bubble)

### Closing conditions (any of these closes the Guide UI):
- `playerInventory` becomes false (closing inventory) -> `InGuideCraftMenu = false` (Main.cs:59338)
- `player.chest != -1` (opening a chest) -> closes guide (Main.cs:54261)
- `npcShop != 0` (opening a shop) -> closes guide
- `talkNPC == -1` (walking away from NPC) -> closes guide
- `InReforgeMenu` becomes true -> closes guide

### dropItemCheck interaction:
When the Guide menu closes, `Player.dropItemCheck()` is called, which returns `Main.guideItem` to the player's inventory. This ensures the material item the player placed in the guide slot doesn't get lost.

### Recipe filtering:
`Recipe.FindRecipes()` (Recipe.cs:484-525): If `guideItem` is not air, calls `CollectGuideRecipes()` which iterates ALL recipes and finds ones where `guideItem` is a required ingredient (including recipe group matching via `useWood`, `useSand`, `useIronBar`, `useFragment`, `usePressurePlate`, `AcceptedByItemGroups`). If `guideItem` is air, normal crafting recipe filtering applies (checks player inventory, nearby tiles, biome conditions).

---

## Implications for Pocket Guide Mod

### Approach: Do NOT reuse vanilla Guide UI
The vanilla Guide UI is deeply coupled to:
1. NPC talk state (`talkNPC != -1` required at all times)
2. Interactive material slot (player physically moves items in/out)
3. Inventory-level rendering (runs inside `DrawInventory()`)
4. The `playerInventory` being open

### What we CAN reuse:
1. **`Recipe.CollectGuideRecipes()` logic** — but it's private. We already replicate this in our mod (scanning all recipes for ingredient matches). The key patterns to match: `IsTheSameAs`, `useWood`, `useSand`, `useIronBar`, `useFragment`, `AcceptedByItemGroups`, `usePressurePlate`.
2. **Localization strings** — `Lang.inter[21]` ("Showing recipes that use"), `Lang.inter[22]` ("Required objects:"), `Lang.inter[23]` ("None") can be reused if we want vanilla-consistent text.
3. **`ItemSlot.Draw` context 22** for drawing recipe result items and ingredient items.
4. **`SetRecipeMaterialDisplayName` pattern** for recipe group name display ("Any Iron Bar", etc.).

### What we need to build ourselves:
1. Custom tooltip/panel overlay rendered in `PostDrawInterface` (our current approach)
2. Static item display (no interactive slot — just show the hovered item's icon and name)
3. Recipe list with item icons, ingredient icons, and crafting station text
4. Positioning relative to cursor (unlike vanilla which is fixed-position in the inventory)
5. Background panel drawing (vanilla uses `TextureAssets.InventoryBack` for slots)

### Key vanilla rendering patterns to reference:
- Recipe result items drawn with `ItemSlot.Draw(spriteBatch, ref item, 22, position)` at 0.6-0.75 scale
- Ingredient items drawn at `inventoryScale = 0.6f`
- Text uses `FontAssets.MouseText.Value` with `DynamicSpriteFontExtensionMethods.DrawString`
- Color is `mouseTextColor`-based (pulsing white text)
- Crafting station names: `Lang.GetMapObjectName(MapHelper.TileToLookup(tileId, Recipe.GetRequiredTileStyle(tileId)))`
