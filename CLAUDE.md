IMPORTANT: Never start coding without explicit user confirmation. Answering a question or discussing a task is NOT permission to code. Always ask "Ready for me to start coding?" (or similar) and wait for a clear "yes" before writing any code or creating/editing files.

# Guide Voodoo Doll Mod

A tModLoader mod that shows recipe info when hovering over items, without needing to talk to the Guide NPC.

## Design

- **"Held" logic**: The doll counts as held if it's in the player's inventory, void bag, or accessory slots — same logic as Guide to Critter Companionship.
- **Toggle**: Right-click to activate/deactivate — same behavior as Guide to Critter Companionship.
- **Data**: When active and hovering over an item, show the same recipe information the Guide NPC provides (what the item crafts into, and what recipes use it as an ingredient).
- **UI**: Custom tooltip UI designed to fit a hover context — the Guide NPC's recipe panel is the content reference, not the layout reference.
