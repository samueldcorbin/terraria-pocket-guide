# Implement Guide Voodoo Doll Toggle Item

## Status: Complete

## What was built
- `PocketGuidePlayer` (ModPlayer) — stores `DollActive` (persisted toggle) and `DollPresent` (per-frame presence flag)
- `PocketGuideGlobalItem` (GlobalItem) — hooks into vanilla Guide Voodoo Doll (ItemID 267):
  - Right-click to toggle active/inactive
  - `UpdateInventory` + `UpdateAccessory` set `DollPresent = true`
  - `PreDrawInInventory` dims the sprite at 50% opacity when inactive

## How vanilla works (from decompiled source)
- `Player.UpdateEquips` loops inventory slots 0-57, calling `RefreshMechanicalAccsFromItemType` — this is how Critter Companionship works from inventory/void bag
- `Player.UpdateEquips` loops accessory slots 3-9, calling `ApplyEquipFunctional` — this is how Guide Voodoo Doll sets `killGuide = true`
- Void bag items get inventory treatment if `ItemID.Sets.WorksInVoidBag[type]` is true
- Guide Voodoo Doll (267): `accessory = true`, `maxStack = CommonMaxStack`

## Design decisions
- Toggle state on ModPlayer (not per-item) — simpler, one toggle per player
- Two bools: `DollActive` (user's choice, persisted) + `DollPresent` (is doll on player this frame)
- Recipe hover (later task) checks `DollActive && DollPresent`
- No changes to vanilla `killGuide` behavior
