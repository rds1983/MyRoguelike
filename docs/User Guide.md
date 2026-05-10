# MyRoguelike — User Guide

> Version 0.5.0 — Phase 5 (Player Movement & Controls)

---

## Introduction

MyRoguelike is a Hack-n-Slash 2D Roguelike. Explore a procedurally generated world, fight enemies, collect loot, and descend into dungeons.

## Installation

**Prerequisites:**
- Windows
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

**Build & Run:**
```sh
cd D:\Projects\MyRoguelike
dotnet run
```

## Controls

| Key | Action |
|-----|--------|
| W / Up Arrow | Move up |
| S / Down Arrow | Move down |
| A / Left Arrow | Move left |
| D / Right Arrow | Move right |
| Enter | Interact (stairs, doors) / Confirm |

## Scenes

### Title Screen
- Game starts at the title screen
- Press **Enter** to begin your adventure

### Overworld
- Navigate the test map using WASD or arrow keys
- Walls ( `#` ) block movement
- Water ( `~` ) and trees ( `T` ) are impassable
- Walk on roads ( `=` ), grass ( `.` ), dirt ( `,` ), doors ( `+` )
- You are represented by the `@` symbol

### Stairs
- Stand on stairs ( `>` down, `<` up ) and press **Enter** to descend or climb
- Press **Enter** on the message screen to return to the overworld
- Full dungeon generation coming in a future update

## Gameplay

*Under active development. The game currently supports:*
- Title screen with "Press Enter to Start"
- Tile-based overworld rendering with colored glyphs
- Player movement with collision detection
- Camera that follows the player
- Stair interaction with placeholder transitions

## Planned Features

- 5 playable classes: Warrior, Mage, Wizard, Cleric, Monk
- Procedurally generated world with history and mythology
- Overworld travel with cities, villages, and dungeons
- 50+ enemies with unique special attacks
- 50+ items, potions, scrolls, and shops
- Turn-based combat

---

*This guide will be expanded as features are implemented.*
