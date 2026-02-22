# Implement Guide Voodoo Doll Toggle Item

## Goal
Create a ModItem that uses the existing Guide Voodoo Doll as its base concept. It sits in inventory, Void Bag, or accessory slot. It has an on/off toggle state (like Void Bag or Guide to Critter Companionship).

## Behavior
- Right-click to toggle on/off
- When toggled on: activates recipe hover lookup system
- When toggled off: inert
- Sprite changes between states (flip/rotate for off state)

## Implementation approach
- Subclass ModItem
- Store toggle state (likely a bool on the ModItem instance or a ModPlayer field)
- Override appropriate hooks for right-click toggle
- Use vanilla Guide Voodoo Doll item texture as base, apply transform for off state
- Needs to work from inventory, Void Bag, and accessory slots

## Open questions
- Should the toggle state persist per-player (ModPlayer) or per-item-instance?
- How does Void Bag detect its toggle state? Mirror that pattern.
- Exact sprite treatment for off state — user mentioned rotate or mirror.
