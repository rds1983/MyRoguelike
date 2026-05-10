# MyRoguelike — User Guide

> Version 0.9.0 — Phase 9 (Dungeons, FOV, Traps)

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
- Explore a **100×100 procedurally generated world** with varied biomes
- Navigate using WASD or arrow keys
- Walls ( `#` ) block movement
- Water ( `~` ) and trees ( `T` ) are impassable
- Walk on roads ( `=` ), grass ( `.` ), dirt ( `,` ), doors ( `+` )
- You are represented by the `@` symbol

### Cities & Villages
- **Cities** (3 per world): Clusters of 4–8 stone buildings with walls, floors, and doors
- **Villages** (4 per world): Smaller clusters of 2–4 buildings
- Buildings have stone walls ( `#` ) and doors ( `+` ) you can walk through
- Walkable interior floors are stone ( `.` )

### Dungeons
- **Dungeons** (3 per world): Stone structures containing stairs down ( `>` )
- Stand on the stairs and press **Enter** to descend into a procedurally generated dungeon floor
- Dungeons are **dark**: you can only see within your field of view (FOV)
- Tiles you have seen before remain faintly visible (explored memory)
- Watch for **spike traps** ( `^` ) — stepping on one deals damage and removes the trap
- Find `stairs_up` ( `<` ) and press **Enter** to return to the overworld

### Roads
- Settlements are connected by roads ( `=` )
- Follow roads to travel between towns safely

### Enemies
- 10 enemies spawn randomly on the overworld at walkable tiles
- Bump into enemies to attack them (turn-based combat)
- Gain XP, gold, and loot from defeated enemies

## Gameplay

*Under active development. The game currently supports:*
- Title screen with "Press Enter to Start"
- 100×100 procedurally generated overworld with 10 biomes
- 3 cities, 4 villages, and 3 dungeons with buildings and roads
- BSP dungeon generation with stairs up/down, FOV, and spike traps
- Player movement with collision detection and bump-to-attack combat
- 10 enemy types on the overworld
- Turn-based combat with XP, gold, and loot drops
- Camera that follows the player
- Stair interaction with placeholder dungeon transitions
- Stair interaction to enter a real dungeon scene
- Message log with color-coded combat and event messages

## Planned Features

- 5 playable classes: Warrior, Mage, Wizard, Cleric, Monk
- Full dungeon generation with multiple floors, rooms, and traps
- 50+ enemies with unique special attacks
- 50+ items, potions, scrolls, shops, and inventory management
- Dungeon FOV and lighting system

---

*This guide will be expanded as features are implemented.*
