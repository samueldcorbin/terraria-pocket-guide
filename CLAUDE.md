IMPORTANT: Never start coding without explicit user confirmation. Answering a question or discussing a task is NOT permission to code. Always ask "Ready for me to start coding?" (or similar) and wait for a clear "yes" before writing any code or creating/editing files.

# Pocket Guide Mod

A tModLoader mod that shows recipe info when hovering over items, without needing to talk to the Guide NPC.

## Design

- **Item**: "Pocket Guide" — a new item crafted from Guide Voodoo Doll + 30 Purification Powder. Not an accessory, inventory-only.
- **Toggle**: Right-click to activate/deactivate — identical behavior to Guide to Critter Companionship (type swap between active/inactive ModItems).
- **Sprite**: Uses Guide to Peaceful Coexistence sprite (placeholder). Inactive variant is rotated 180°.
- **Data**: When active and hovering over an item, show the same recipe information the Guide NPC provides (what the item crafts into, and what recipes use it as an ingredient).
- **UI**: Custom tooltip UI designed to fit a hover context — the Guide NPC's recipe panel is the content reference, not the layout reference.
