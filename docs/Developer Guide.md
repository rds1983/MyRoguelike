# MyRoguelike — Developer Guide

> Version 0.5.0 — Phase 5 (Player Movement & Controls)

---

## Prerequisites

- Windows 10+
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [MonoGame 3.8+](https://community.monogame.net/) (installed via NuGet)

## Build & Run

```sh
cd D:\Projects\MyRoguelike

# Restore dependencies
dotnet restore MyRoguelike.slnx

# Build
dotnet build MyRoguelike.slnx

# Run (from project directory)
dotnet run --project src\MyRoguelike

# Publish (self-contained)
dotnet publish src\MyRoguelike\MyRoguelike.csproj -c Release -r win-x64 --self-contained
```

## Project Structure

```
MyRoguelike/
├── MyRoguelike.slnx              # Solution file (root only)
├── src/
│   └── MyRoguelike/              # The game project
│       ├── MyRoguelike.csproj    # .NET 8.0 project file (WindowsDX)
│       ├── app.manifest          # Windows compatibility manifest
│       ├── Icon.ico              # Game icon
│       ├── dotnet-tools.json     # Local tool manifest (mgcb)
│       ├── Content/              # MonoGame Content Pipeline
│       │   ├── Content.mgcb
│       │   ├── Textures/
│       │   ├── Fonts/
│       │   └── Audio/
│       ├── Data/                 # Data system code (DataManager, Models)
│       ├── Json/                 # JSON game data files
│       ├── Saves/                # Runtime save files (generated)
│       ├── Core/                 # Core engine files
│       │   ├── Game1.cs
│       │   ├── Program.cs
│       │   ├── Constants.cs
│       │   └── Rng.cs
│       ├── Data/Models/          # C# POCOs for JSON data
│       ├── Data/Converters/      # Custom JSON converters
│       ├── Entities/             # Player, Enemy, NPC
│       ├── Components/           # Stats, Inventory, Equipment, AI, etc.
│       ├── World/                # World generation & map data
│       ├── Systems/              # Combat, AI, FOV, pathfinding, etc.
│       ├── Scenes/               # Title, Overworld, Dungeon, Shop, etc.
│       ├── UI/                   # HUD, menus, tooltips
│       └── Save/                 # Save/load system
└── docs/                         # All documentation
    ├── vision.md                 # Project vision & requirements
    ├── design.md                 # Comprehensive design document
    ├── roadmap.md                # Development roadmap (23 phases)
    ├── User Guide.md             # User-facing manual
    ├── Developer Guide.md        # Developer documentation (this file)
    └── state.json                # Project state tracker (for agent handoff)
```

## Architecture Overview

**Pattern:** Entity-Component composition (not full ECS). `Entity` is a base class with a `List<IComponent>`. Systems operate on entities by querying components.

**Scene Management:** Stack-based (`SceneManager`). Each scene (title, overworld, dungeon, shop) has its own `Update`/`Draw` lifecycle. `Game1.Instance.SceneManager` provides access from any scene.

**Data Pipeline:** All game content defined in JSON files under `src/MyRoguelike/Json/`. Loaded once at startup via `DataManager`. Strongly typed C# models in `src/MyRoguelike/Data/Models/`.

## Core Files

### `Game1.cs`
Main MonoGame `Game` subclass. Initializes graphics, loads content, runs the game loop. Exposes `Instance` singleton and `SceneManager` property for scene access.

### `Constants.cs`
Central constants file. Tile size (32×32), screen dimensions (1280×720), game title, and other magic numbers.

### `Rng.cs`
Seeded random number generator wrapping `System.Random`. All procedural generation uses this for deterministic output.

## Data System

All game content is defined in JSON files and loaded via `DataManager`.

### JSON Schema Reference

| File | Schema Model | Location |
|------|-------------|----------|
| `classes.json` | `ClassDef` | 5 player classes with stats, growth, skills, equipment |
| `enemies.json` | `EnemyDef` | Enemy definitions with stats, loot, AI behavior |
| `items.json` | `ItemDef` | Item definitions with category, stats, rarity |
| `potions.json` | `PotionDef` | Consumable potion effects |
| `scrolls.json` | `ScrollDef` | Single-use scroll magic effects |
| `tiles.json` | `TileDef` | Tile types for map rendering |
| `loot_tables.json` | `LootTableDef` | Weighted loot drop tables |
| `special_attacks.json` | `SpecialAttackDef` | Unique enemy attack definitions |

### DataManager

- Located in `src/MyRoguelike/Data/DataManager.cs`
- Called from `Game1.LoadContent()`: reads all 8 JSON files from output `Json/` directory
- JSON files are auto-copied to output via `.csproj` `<Content>` items with `CopyToOutputDirectory`
- Uses `System.Text.Json` with `PropertyNameCaseInsensitive = true`
- Stores data in `Dictionary<string, T>` keyed by `id` for O(1) lookup
- Provides `GetX(string id)` accessors for each data type
- Validates cross-references (loot tables, starting equipment, special attacks) at load time

### Adding New Data

1. Add the entry to the appropriate JSON file in `src/MyRoguelike/Json/`
2. If a new data type is needed, create a C# model class in `src/MyRoguelike/Data/Models/`
3. Add loading logic in `DataManager.LoadAll()` and a `GetX()` accessor
4. Build and verify: `dotnet build MyRoguelike.slnx`

### Model Classes

All C# models are in `src/MyRoguelike/Data/Models/` with names matching JSON structure:
- `StatBlock` — STR/DEX/CON/INT/WIS stats
- `ClassDef`, `EnemyDef`, `ItemDef`, `PotionDef`, `ScrollDef`, `TileDef`
- `LootTableDef` / `LootEntry` — weighted loot tables
- `SpecialAttackDef` — special attack definitions
- `IntRange`, `ColorDef` — supporting types
- `StartingEquipmentDef` — class starting gear

## Entity System & Components

### Architecture

Entities use **composition over inheritance**. `Entity` is a base class with a `List<IComponent>`. Behavior is added by attaching components.

```
Entity
├── StatsComponent      # HP, MP, STR/DEX/CON/INT/WIS
├── InventoryComponent  # Item storage (capacity-limited)
├── EquipmentComponent  # Equipped gear (weapon, armor, shield, 2x accessory)
├── CombatComponent     # ATK, DEF, hit/crit calculations
├── AiComponent         # Behavior type + state machine
└── EffectComponent     # Active status effects (buffs/debuffs)
```

### Entity Subclasses

| Class | Extends | Purpose |
|-------|---------|---------|
| `Player` | `Entity` | Player-specific: class, XP, level, gold |
| `Enemy` | `Entity` | Enemy-specific: def reference, loot generation |
| `Npc` | `Entity` | Non-player characters: dialogue, shop inventory |

### Entity Properties (Phase 5 additions)

All entities now have:
- `Position` (`Microsoft.Xna.Framework.Point`) — tile coordinates on the map
- `Glyph` (`string`) — display character (default `"@"`)
- `Color` (`Microsoft.Xna.Framework.Color`) — render color (default white)

### Components

#### `StatsComponent`
- `SetHp(max, current?)` / `SetMp(max, current?)`
- `ApplyDamage(amount)` — returns actual damage dealt
- `Heal(amount)` / `RestoreMana(amount)` — returns actual restored
- `BaseStats` + `BonusStats` → `TotalStrength`, `TotalDexterity`, etc.
- `AddBonusStats()` / `RemoveBonusStats()` for equipment/effect modifiers

#### `InventoryComponent`
- `Capacity` (default 40), `AddItem()`, `RemoveItem()`, `HasItem()`
- Supports stacking for potions/scrolls/materials
- `CountItem(id)` returns total quantity across stacks

#### `EquipmentComponent`
- Slots: Weapon, Armor, Shield, Accessory1, Accessory2
- `Equip(item)` — auto-selects slot based on item category
- `Unequip(item)` / `UnequipSlot(slot)`
- `GetTotalStatBonuses()` → `StatBlock` of STR/DEX/CON/INT/WIS
- `GetAllBonuses()` → full `Dictionary<string, int>` of all stat modifiers
- `GetAllEquipped()` — enumerates all equipped items

#### `CombatComponent`
- `Recalculate(stats, equipment)` — computes ATK, MATK, DEF from stats + gear
- `RollHit(targetEvasion)` — hit check using accuracy/evasion formula
- `RollCrit()` — critical hit check
- `CalculateDamage(atk, def)` — damage = max(1, atk - def)

#### `AiComponent`
- `BehaviorType`: aggressive, defensive, cowardly, pack, boss
- `CurrentState`: idle, alert, attacking, fleeing, patrolling
- `DetectionRange`: how far the AI detects the player

#### `EffectComponent`
- `ApplyEffect(ActiveEffect)` — refreshes duration if duplicate
- `TickEffects()` — decrements turns, removes expired effects
- `HasEffect(id)`, `RemoveEffect(id)`, `Clear()`

### Runtime Item

`Item` is the runtime representation of an `ItemDef`:
- `Id` matches `ItemDef.Id`, `Def` is the definition reference
- `Quantity` for stackable items, `IsIdentified` for unidentified gear
- `DisplayName` falls back to `Def.Name` or `Id`
- `IsStackable` set based on item category (potion, scroll, material)

### Creating an Entity

```csharp
var goblin = new Enemy { Id = "goblin_01", Name = "Goblin Scout", EnemyDefId = "goblin_scout" };
goblin.AddComponent<StatsComponent>();
goblin.AddComponent<AiComponent>();
goblin.AddComponent<EffectComponent>();

var stats = goblin.GetComponent<StatsComponent>()!;
stats.BaseStats = Game1.Data.GetEnemy("goblin_scout")!.Stats;
stats.SetHp(Game1.Data.GetEnemy("goblin_scout")!.Hp);
```

## Scenes

### Scene Lifecycle

Each scene extends `Scene` (abstract base in `src/MyRoguelike/Scenes/Scene.cs`):
1. `LoadContent()` — called once when pushed onto the stack
2. `Update(GameTime)` — called every frame when this scene is on top
3. `Draw(SpriteBatch, GameTime)` — called every frame when this scene is on top

### Scene Transitions

Scenes use `Game1.Instance.SceneManager` to transition:
- `Push(scene)` — suspends current scene, starts new one
- `Pop()` — returns to previous scene
- `Clear()` — removes all scenes (used for game over → title)

### Scene List

| Scene | File | Purpose |
|-------|------|---------|
| `TitleScene` | `Scenes/TitleScene.cs` | "Press Enter to Start" — entry point |
| `OverworldScene` | `Scenes/OverworldScene.cs` | Main game map with player movement |
| `PlaceholderScene` | `Scenes/PlaceholderScene.cs` | Generic message scene (used by stairs) |
| `GameOverScene` | `Scenes/GameOverScene.cs` | "You Died" — press Enter to return to title |

## Input & Movement (Phase 5)

### Input Handling

Input is polled in `OverworldScene.Update()` using `Keyboard.GetState()` each frame:

```csharp
var kb = Keyboard.GetState();

if (kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.Up)) dy = -1;
else if (kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Down)) dy = 1;
else if (kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.Left)) dx = -1;
else if (kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Right)) dx = 1;
```

### Movement Flow

1. Player presses WASD or Arrow key
2. `TryMovePlayer(dx, dy)` computes new position
3. `Map.IsInBounds(newX, newY)` — stops at map edges
4. `Map.IsWalkable(newX, newY)` — checks tile def `IsWalkable` property
5. If both pass, `Entity.Position` is updated
6. Camera follows: `_camera.CenterOn(player.X, player.Y)`

### Stair Interaction

When the player presses Enter on a `stairs_down` or `stairs_up` tile:
- A `PlaceholderScene` is pushed onto the stack with a message
- Pressing Enter on the placeholder pops back to the overworld
- This mechanism will be replaced with actual `DungeonScene` transitions in Phase 9

### Adding New Scenes

1. Create a class extending `Scene` in `src/MyRoguelike/Scenes/`
2. Override `LoadContent()`, `Update()`, `Draw()` as needed
3. Push to the scene manager:
   ```csharp
   Game1.Instance.SceneManager.Push(new MyScene());
   ```
4. Draw text using `Game1.Font` (SpriteFont loaded from content pipeline)

## Content Pipeline

MonoGame's Content Pipeline is used via `Content.mgcb`. Content is added using the MGCB Editor tool:

```sh
# Install the content builder tool (already done)
dotnet tool restore

# Build content (runs automatically during dotnet build)
dotnet build
```

### Fonts

- Font descriptor at `Content/Fonts/Console.spritefont` (Arial, 24pt)
- Referenced in `Content.mgcb` with `FontDescriptionImporter` + `FontDescriptionProcessor`
- Loaded at runtime: `Content.Load<SpriteFont>("Fonts/Console")`
- Available globally: `Game1.Font`

## Adding New Source Files

1. Create the `.cs` file in the appropriate `src/MyRoguelike/` subdirectory
2. Use namespace `MyRoguelike` (or sub-namespace)
3. Build with `dotnet build MyRoguelike.slnx` — all `.cs` files in the project are auto-included

## Current Status

- **Phase:** 5 (Player Movement & Controls) — Complete
- **Next phase:** 6 (Combat System)
- For full task breakdown, see [`docs/roadmap.md`](docs/roadmap.md)
- For design details, see [`docs/design.md`](docs/design.md)

## Testing

The project uses **xUnit** for unit tests. Test project is at `tests/MyRoguelike.Tests/`.

### Running Tests

```sh
# From solution root
dotnet test MyRoguelike.slnx

# Run with verbose output
dotnet test MyRoguelike.slnx -v n

# Run a specific test class
dotnet test MyRoguelike.slnx --filter "WorldSceneTests"
```

### Adding Tests

1. Add test files to `tests/MyRoguelike.Tests/`
2. Use `[Fact]` for single tests, `[Theory]` with `[InlineData]` for parameterized tests
3. The test project auto-copies JSON data files from the main project's `Json/` directory
4. Build and run: `dotnet test MyRoguelike.slnx`

### Test Structure
- `DataManagerTests` — 45 tests covering data loading, validation, and content correctness
- `EntityComponentTests` — 59 tests covering entities, components, and runtime items
- `WorldSceneTests` — 28 tests covering Map, Camera, SceneManager, Tile

## Troubleshooting

| Issue | Solution |
|-------|----------|
| `dotnet mgcb` not found | Run `dotnet tool restore` to restore local tools |
| Ambiguous `Color`/`Keys` types | Use fully qualified `Microsoft.Xna.Framework.Color` or `Keys = Microsoft.Xna.Framework.Input.Keys` |
| Ambiguous `Point` type | Use fully qualified `Microsoft.Xna.Framework.Point` |
| Content build fails | Verify `Content/Content.mgcb` exists and MGCB tool is installed |
