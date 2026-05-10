# MyRoguelike

A **Hack-n-Slash 2D Roguelike** built with C#, .NET 8.0, and MonoGame.

> **Current Version:** 0.7.0 — Phase 7 (World Generator)

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 8.0 (WindowsDX) |
| Game Framework | MonoGame 3.8.2 |
| Data | System.Text.Json / JSON files |
| Rendering | SpriteBatch, tile-based (32×32), SpriteFont glyphs |
| Architecture | Entity-Component composition + stack-based SceneManager |

## Quick Start

```sh
git clone <repo-url>
cd MyRoguelike
dotnet run
```

**Prerequisites:** Windows, [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Controls

| Key | Action |
|-----|--------|
| W / Up Arrow | Move up / bump attack |
| S / Down Arrow | Move down / bump attack |
| A / Left Arrow | Move left / bump attack |
| D / Right Arrow | Move right / bump attack |
| Space / . | Wait (pass turn) |
| Enter | Interact (stairs, doors) / Confirm |

Walk into an enemy to **bump attack**. Killing enemies grants XP, gold, and loot.

## Project Structure

```
MyRoguelike/
├── src/MyRoguelike/
│   ├── Core/           # Game1, Program, Constants, Rng
│   ├── Components/     # Stats, Inventory, Equipment, Combat, AI, Effects
│   ├── Entities/       # Entity, Player, Enemy, Npc
│   ├── Systems/        # CombatSystem, TurnSystem, AiSystem, PathfindingSystem, EventSystem, NameGenerator
│   ├── Scenes/         # SceneManager + Title, Overworld, Placeholder, GameOver scenes
│   ├── UI/             # MessageLog
│   ├── World/          # Map, Tile, Camera, World, Region, BiomeGenerator, WorldGenerator
│   ├── Data/           # DataManager + JSON models + Converters
│   ├── Json/           # Data files (classes, enemies, items, potions, scrolls, tiles, loot, special attacks)
│   └── Content/        # MonoGame content pipeline (Fonts)
├── tests/              # xunit tests (185 passing)
└── docs/               # Design doc, roadmap, state tracker, user & developer guides
```

## Roadmap

| Phase | Status |
|-------|--------|
| 1-2 | Project scaffold, data system | ✅ |
| 3 | Entity system & components | ✅ |
| 4 | Rendering engine & tile map | ✅ |
| 5 | Player movement & controls | ✅ |
| 6 | Combat system | ✅ |
| 7 | World generator (terrain) | ✅ |
| 8+ | Settlements, dungeons, items, shops, etc. | 🔜 |

See [`docs/roadmap.md`](docs/roadmap.md) for the full 23-phase plan.

## Development

```sh
dotnet build
dotnet test
dotnet run
```

All game data is driven by JSON files in `src/MyRoguelike/Json/`. The project state is tracked in [`docs/state.json`](docs/state.json).
