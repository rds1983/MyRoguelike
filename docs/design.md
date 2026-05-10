# MyRoguelike — Design Document

> A Hack-n-Slash 2D Roguelike built with C#, .NET 8.0, and MonoGame.

---

## 1. Architecture Overview

### 1.1 Technology Stack
| Layer | Technology |
|-------|-----------|
| Runtime | .NET 8.0 |
| Game Framework | MonoGame 3.8+ |
| Data Serialization | System.Text.Json |
| Input | MonoGame Keyboard + Mouse |
| Rendering | MonoGame SpriteBatch (2D, tile-based) |

### 1.2 High-Level Architecture
The game follows an **Entity-Component** pattern (not full ECS — simple composition on `GameObject`). The engine loop is a standard MonoGame `Game` subclass.

```
Game1 (MonoGame Game)
├── SceneManager (manages scenes: title, overworld, dungeon, shop, etc.)
│   ├── TitleScene
│   ├── OverworldScene
│   ├── DungeonScene
│   ├── ShopScene
│   └── CombatScene
├── ContentManager (loads textures, fonts, sounds)
├── DataManager (reads/writes JSON data files)
├── WorldGenerator (procedural world + history)
└── SaveManager (serializes state.json)
```

### 1.3 Game Loop (per frame)
1. `Update(gameTime)` — SceneManager delegates to active scene
2. `Draw(gameTime)` — SceneManager delegates to active scene
3. All systems (AI, combat, particle effects) tick via the active scene

---

## 2. Project Structure

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
│       ├── Data/                 # JSON data files (game content)
│       │   ├── classes.json
│       │   ├── enemies.json
│       │   ├── items.json
│       │   ├── potions.json
│       │   ├── scrolls.json
│       │   ├── tiles.json
│       │   ├── loot_tables.json
│       │   └── special_attacks.json
│       ├── Saves/                # Runtime saves (generated)
│       │   └── world_*.json
│       ├── Core/
│       │   ├── Game1.cs
│       │   ├── Program.cs
│       │   ├── Constants.cs
│       │   └── Rng.cs            # Seeded RNG wrapper
│       ├── Data/
│       │   ├── DataManager.cs    # Loads all JSON data
│       │   ├── Models/           # C# POCOs for JSON data
│       │   │   ├── ClassDef.cs
│       │   │   ├── EnemyDef.cs
│       │   │   ├── ItemDef.cs
│       │   │   ├── PotionDef.cs
│       │   │   ├── ScrollDef.cs
│       │   │   ├── TileDef.cs
│       │   │   ├── LootTableDef.cs
│       │   │   └── SpecialAttackDef.cs
│       │   └── Converters/       # Custom JsonConverters
│       ├── Entities/
│       │   ├── Entity.cs         # Base entity
│       │   ├── Player.cs         # Player-specific (class, XP, level)
│       │   ├── Enemy.cs          # Enemy AI, loot drop
│       │   └── Npc.cs            # Shopkeeper, questgiver
│       ├── Components/
│       │   ├── StatsComponent.cs
│       │   ├── InventoryComponent.cs
│       │   ├── EquipmentComponent.cs
│       │   ├── CombatComponent.cs
│       │   ├── AiComponent.cs
│       │   └── EffectComponent.cs
│       ├── World/
│       │   ├── World.cs
│       │   ├── Region.cs
│       │   ├── Tile.cs
│       │   ├── WorldGenerator.cs
│       │   ├── BiomeGenerator.cs
│   │   ├── CityGenerator.cs
│   │   ├── DungeonGenerator.cs
│   │   ├── HistoryGenerator.cs # Generates world history / mythology
│   │   └── NameGenerator.cs    # Fantasy name generator
│   ├── Systems/
│   │   ├── CombatSystem.cs
│   │   ├── TurnSystem.cs
│   │   ├── AiSystem.cs
│   │   ├── FovSystem.cs
│   │   ├── PathfindingSystem.cs
│   │   ├── ParticleSystem.cs
│   │   └── EventSystem.cs
│   ├── Scenes/
│   │   ├── Scene.cs
│   │   ├── SceneManager.cs
│   │   ├── TitleScene.cs
│   │   ├── OverworldScene.cs
│   │   ├── DungeonScene.cs
│   │   ├── ShopScene.cs
│   │   ├── InventoryScene.cs
│   │   ├── CharacterScene.cs
│   │   └── GameOverScene.cs
│   ├── UI/
│   │   ├── UiManager.cs
│   │   ├── Hud.cs
│   │   ├── MessageLog.cs
│   │   ├── Menu.cs
│   │   └── Tooltip.cs
│   └── Save/
│       ├── SaveManager.cs
│       ├── SaveData.cs
│       └── SaveMetadata.cs
└── docs/                         # All documentation
    ├── vision.md                 # Project vision & requirements
    ├── design.md                 # This document
    ├── roadmap.md                # Development roadmap
    ├── User Guide.md             # User-facing manual
    ├── Developer Guide.md        # Developer documentation
    └── state.json                # Project state tracker
```

---

## 3. Gameplay Systems

### 3.1 Turn-Based Combat

- **Action Queue**: Each entity has a speed stat. Higher speed = more frequent turns.
- **Actions per turn**: Move, Attack, Use Item, Use Skill, Defend, Wait.
- **Attack resolution**: `hitChance = attacker.accuracy / (attacker.accuracy + defender.evasion)`. Roll d100.
- **Damage**: `damage = attacker.atk - defender.def` (minimum 1). Crit doubles ATK (5% base chance, modified by gear).
- **Special Attacks**: Enemies and players can use special attacks with cooldowns. See Section 6.

### 3.2 Overworld Travel

- Tile-based top-down view.
- Different **biomes**: Plains, Forest, Mountains, Desert, Swamp, Tundra.
- Travel time: moving to adjacent tiles costs a small time increment (affects random encounters).
- Random encounters: step on a tile with a chance to trigger combat based on tile type and player level.
- Discover cities, villages, and dungeons as you explore.
- Fog of war: undiscovered tiles are hidden.
- Fast travel: once discovered, cities and villages can be fast-traveled to.

### 3.3 Dungeons

- Procedurally generated each visit (seed-based, deterministic for the same seed).
- Multiple floors (depth = difficulty).
- Rooms connected by corridors.
- Traps, treasure rooms, monster spawners, boss on final floor.
- Lighting system: torches, radius of visibility.

### 3.4 Leveling & Classes

- XP from kills. Level cap: 50.
- Each level: +HP, +MP, stat points to distribute.
- Class determines starting stats, stat growth, and unique skills.

#### 3.4.1 Playable Classes

| Class   | Primary | Secondary | Armor | Weapon     | Role              |
|---------|---------|-----------|-------|------------|-------------------|
| Warrior | STR     | CON       | Heavy | Melee      | Frontline tank    |
| Mage    | INT     | WIS       | Cloth | Staff      | Ranged DPS        |
| Wizard  | INT     | DEX       | Cloth | Staff/Wand | Utility/CC        |
| Cleric  | WIS     | CON       | Medium| Mace       | Healer/Support    |
| Monk    | DEX     | STR       | Cloth | Unarmed    | Fast melee DPS    |

**Warrior Skills:** Power Strike, Shield Bash, Taunt, Whirlwind, War Cry  
**Mage Skills:** Fireball, Ice Lance, Arcane Bolt, Meteor Storm, Mana Shield  
**Wizard Skills:** Magic Missile, Teleport, Charm, Invisibility, Time Stop  
**Cleric Skills:** Heal, Bless, Smite, Holy Shield, Resurrection  
**Monk Skills:** Flurry of Blows, Roundhouse Kick, Meditation, Evasion, Chi Blast  

### 3.5 Shops

- Each city/village has a shop with a random inventory based on location tier.
- Buy items (gold), sell items (50% of base price).
- Shopkeeper is an NPC with dialogue.
- Item categories: weapons, armor, potions, scrolls, accessories.

### 3.6 Items

> At least 50 unique items across these categories:

| Category   | Examples |
|------------|----------|
| Swords     | Iron Sword, Flamebrand, Frostbite, Void Blade |
| Axes       | Battle Axe, Greataxe, Berserker's Cleaver |
| Maces      | Bone Crusher, Cleric's Gavel, Morning Star |
| Staves     | Apprentice Staff, Archmage Staff, Elder Wand |
| Bows       | Shortbow, Longbow, Elven Bow |
| Daggers    | Shadow Dagger, Poison Fang, Ritual Knife |
| Armor      | Leather Armor, Chainmail, Plate Armor, Robe of Power |
| Shields    | Wooden Shield, Iron Shield, Aegis of Faith |
| Accessories| Ring of Protection, Amulet of Health, Cloak of Invisibility |
| Materials  | Dragon Scale, Phoenix Feather, Mana Crystal |

### 3.7 Potions & Scrolls

**Potions (consumable):**
- Health Potion (restores HP)
- Mana Potion (restores MP)
- Strength Potion (+STR for 5 turns)
- Speed Potion (+DEX for 5 turns)
- Invisibility Potion (invisible for 10 turns)
- Antidote (cures poison)
- Full Heal (cures all status + full HP)

**Scrolls (single-use magic):**
- Scroll of Fireball (AoE fire damage)
- Scroll of Teleportation (escape dungeon)
- Scroll of Identify (reveal item properties)
- Scroll of Enchantment (temporarily buffs equipment)
- Scroll of Summoning (summon ally)
- Scroll of Curse (debuff enemy)

---

## 4. World Generation

### 4.1 Overview

World generation is **seed-based** and occurs in phases:

1. **Terrain Generation** — Heightmap → biomes → tile map
2. **Settlement Placement** — Cities, villages, dungeons placed based on biome rules
3. **History Generation** — Simulated timeline of events, wars, heroes, disasters
4. **Mythology Generation** — Pantheon of gods, creation myth, legends

### 4.2 Terrain Generation (Perlin Noise)

- Multi-octave Perlin noise produces a heightmap.
- Height → biome mapping:
  - Deep water: ocean (impassable)
  - Shallow water: coast
  - Low: plains, swamp
  - Mid: forest, hills
  - High: mountains

### 4.3 Settlements

- **Cities** (3–5 per world): Large, multiple shops, quest hub, surrounding walls.
- **Villages** (10–20 per world): Small, 1 shop, simple quests.
- **Dungeons** (15–30 per world): 3–10 floors each, boss at bottom.
- Placement rules:
  - Cities near water or crossroads.
  - Villages in resource-rich biomes.
  - Dungeons in mountains, forests, or underground.

### 4.4 History Generation

Inspired by Dwarf Fortress. A timeline of events is simulated:

1. **Creation Era** — Gods create the world.
2. **Age of Myth** — First civilizations rise and fall.
3. **Age of Heroes** — Legendary figures, wars, great beasts.
4. **Recent Age** — Current state of the world.
5. **Player Arrival** — The player enters the world.

Each event creates artifacts, ruins, named NPCs, and world state changes. Events are stored as a chronological list.

**Example events:**
- Year 124: "The Dragonlord Kael'thar burns the city of Thornhaven."
- Year 342: "The hero Aldric slays the Lich King, founding the Kingdom of Dawn."
- Year 567: "A great plague sweeps the eastern plains, wiping out three villages."

### 4.5 Mythology Generation

- Pantheon of 5–8 procedurally named gods.
- Each god has a domain (War, Nature, Death, Knowledge, Trickery, etc.).
- Creation myth: how the world was formed.
- Legends: stories tying gods to historical events.

---

## 5. Data Format (JSON)

All game data is stored in `Data/*.json`. Example schemas:

### 5.1 classes.json

```json
[
  {
    "id": "warrior",
    "name": "Warrior",
    "description": "A master of melee combat.",
    "baseStats": { "strength": 12, "dexterity": 8, "constitution": 14, "intelligence": 6, "wisdom": 8 },
    "statGrowth": { "strength": 3, "dexterity": 2, "constitution": 3, "intelligence": 1, "wisdom": 1 },
    "hpPerLevel": 12,
    "mpPerLevel": 3,
    "skills": ["power_strike", "shield_bash", "taunt", "whirlwind", "war_cry"],
    "allowedArmor": ["cloth", "leather", "chain", "plate"],
    "allowedWeapons": ["sword", "axe", "mace"],
    "startingEquipment": { "weapon": "iron_sword", "armor": "leather_armor" }
  }
]
```

### 5.2 enemies.json

```json
[
  {
    "id": "goblin_scout",
    "name": "Goblin Scout",
    "tier": 1,
    "stats": { "strength": 6, "dexterity": 10, "constitution": 8, "intelligence": 5, "wisdom": 4 },
    "hp": 15,
    "mp": 0,
    "xpReward": 10,
    "goldReward": { "min": 1, "max": 5 },
    "lootTable": "goblin",
    "abilities": ["stab"],
    "specialAttack": null,
    "spawnBiomes": ["plains", "forest", "mountains"],
    "behavior": "aggressive"
  }
]
```

### 5.3 items.json

```json
[
  {
    "id": "iron_sword",
    "name": "Iron Sword",
    "category": "weapon",
    "subcategory": "sword",
    "tier": 1,
    "stats": { "attack": 5 },
    "value": 50,
    "description": "A sturdy iron blade."
  }
]
```

### 5.4 potions.json / scrolls.json

```json
{
  "id": "health_potion",
  "name": "Health Potion",
  "effectType": "heal",
  "effectValue": 30,
  "value": 25,
  "description": "Restores 30 HP."
}
```

### 5.5 special_attacks.json

```json
{
  "id": "dragon_breath",
  "name": "Dragon Breath",
  "type": "cone_aoe",
  "element": "fire",
  "damageMultiplier": 3.0,
  "range": 5,
  "cooldown": 4,
  "description": "Breathes a cone of fire dealing 3x ATK damage."
}
```

### 5.6 loot_tables.json

```json
{
  "id": "goblin",
  "entries": [
    { "itemId": "gold", "weight": 50, "min": 1, "max": 5 },
    { "itemId": "rusty_dagger", "weight": 20, "min": 1, "max": 1 },
    { "itemId": "health_potion", "weight": 10, "min": 1, "max": 1 },
    { "itemId": "goblin_ear", "weight": 15, "min": 1, "max": 2 }
  ]
}
```

---

## 6. Enemies (50+)

### 6.1 Tier 1 (Early) — 10 enemies
Goblin Scout, Giant Rat, Slime, Skeleton, Bat, Spider, Wolf, Bandit, Mushroom Man, Fire Sprite

### 6.2 Tier 2 (Mid) — 15 enemies
Orc Warrior, Dark Elf, Werewolf, Wraith, Harpy, Basilisk, Troll, Golem, Frost Spider, Shadow, Cultist, Treant, Manticore, Lizardman, Cave Ogre

### 6.3 Tier 3 (Late) — 15 enemies
Vampire, Lich, Demon, Chimera, Hydra, Beholder, Fire Giant, Frost Giant, Storm Elemental, Dracolich, Mind Flayer, Death Knight, Succubus, Naga Queen, Abyssal Lord

### 6.4 Bosses — 10 enemies
Dragon (fire/ice/void variants), Kraken, Titan, Archdemon, Eldritch Horror, Phoenix, Leviathan, Necromancer Lord, Colossus, The Void Walker

### 6.5 Special NPC Attacks
| Enemy | Special Attack | Effect |
|-------|---------------|--------|
| Dragon | Dragon Breath | Cone AoE fire damage |
| Crocodile | Death Roll | Grapple + bleed DoT |
| Basilisk | Petrifying Gaze | Stun 3 turns (save vs CON) |
| Lich | Soul Drain | Steals HP+MP, heals self |
| Vampire | Blood Drain | Restores own HP equal to damage |
| Harpy | Siren Song | Confuses target (random move) |
| Beholder | Disintegration Ray | Massive single-target damage |
| Treant | Entangle | Root in place 2 turns |
| Wraith | Possess | Take control of enemy minion |
| Hydra | Multi-Head Strike | Hits 3 random targets |
| Spider | Web Trap | Immobilize + poison |
| Golem | Ground Pound | AoE stun around self |

---

## 7. Items (50+)

### 7.1 Weapons (20+)
Rusty Dagger, Iron Sword, Battle Axe, War Hammer, Shortbow, Longbow, Apprentice Staff, Shadow Dagger, Flamebrand, Frostbite, Void Blade, Greataxe, Bone Crusher, Cleric's Gavel, Archmage Staff, Elven Bow, Berserker's Cleaver, Morning Star, Elder Wand, Doomhammer, Blade of the Ancients

### 7.2 Armor (12)
Tattered Cloth, Leather Armor, Chainmail, Plate Armor, Robe of Power, Shadow Leather, Mithril Chain, Dragon Scale Armor, Cloak of Flames, Guardian Plate, Ethereal Robe, Aegis of Faith

### 7.3 Shields (5)
Wooden Shield, Iron Shield, Kite Shield, Tower Shield, Aegis

### 7.4 Accessories (8)
Ring of Protection, Amulet of Health, Cloak of Invisibility, Ring of Strength, Amulet of Wisdom, Boots of Speed, Bracers of Defense, Crown of Command

### 7.5 Materials / Quest Items (10)
Dragon Scale, Phoenix Feather, Mana Crystal, Lich's Heart, Unicorn Horn, Void Shard, Titan's Blood, Fairy Dust, Ancient Relic, Golden Idol

---

## 8. User Interface

### 8.1 HUD (always visible)
- HP/MP bars
- Level & XP bar
- Active effects (buffs/debuffs icons)
- Minimap (top-right corner)
- Gold counter
- Current floor / location name

### 8.2 Message Log
- Bottom-left, scrollable
- Shows combat messages, item pickups, events
- Color-coded: white (info), yellow (loot), red (damage), green (heal), blue (magic)

### 8.3 Menus
- **Character** (C): Stats, equipment, skills
- **Inventory** (I): Items, use, equip, drop
- **Map** (M): Full overworld map if explored
- **Log** (L): Full message history
- **Escape**: Pause menu (save, load, settings, quit)

### 8.4 Controls
| Key | Action |
|-----|--------|
| Arrow keys / WASD | Move / navigate menus |
| Enter / Space | Confirm / interact |
| Escape | Pause / back |
| Tab | Cycle targets |
| 1-9 | Quick-use items |
| C | Character sheet |
| I | Inventory |
| M | Map |
| L | Message log |
| Shift + direction | Attack in direction |

---

## 9. Save System

- **Auto-save** on entering/leaving dungeons, shops, and resting.
- **Manual save** via pause menu.
- Save format: JSON (`Saves/world_<timestamp>.json`).
- Contents: player state, world seed, explored tiles, inventory, quest progression, history timeline index.

---

## 10. Procedural Content Design

### 10.1 Name Generation
- Syllable-based fantasy name generator.
- Used for: characters, cities, dungeons, gods, items.

### 10.2 Dungeon Generation
- BSP (Binary Space Partitioning) for room placement.
- Corridors connect rooms via A*.
- Room types: spawn, monster, treasure, trap, shop, boss.

### 10.3 Loot System
- Loot tables with weighted entries.
- Item tier scales with dungeon depth / enemy tier.
- Rarity: Common, Uncommon, Rare, Epic, Legendary.
- Magic items have prefix/suffix modifiers (e.g., "Burning Iron Sword of the Giants").

---

## 11. Development Roadmap

See [`docs/roadmap.md`](roadmap.md) for the full detailed phase-by-phase task breakdown (23 phases, ~167 tasks).

**Summary phases:**
1. Project Scaffold & Core Infrastructure
2. Data System (JSON Loading)
3. Entity System & Components
4. Rendering Engine & Tile Map
5. Player Movement & Controls
6. Combat System
7. World Generator (Terrain)
8. Settlement Generation
9. Dungeon Generator
10. Items & Inventory
11. Shops & Economy
12. Special Attacks, Potions & Scrolls
13. Overworld Travel & Encounters
14. History & Mythology Generation
15. All 50+ Enemies
16. All 50+ Items & Loot Balancing
17. UI Polish & Menus
18. Save / Load System
19. Sound & Particles
20. Balance Pass & Polish
21. User Guide
22. Developer Guide
23. Final Integration & Release

Every phase ends with updates to `User Guide.md`, `Developer Guide.md`, and `state.json`.

---

## 12. State Tracking (state.json)

The file `docs/state.json` contains a comprehensive snapshot:

```json
{
  "project": "MyRoguelike",
  "version": "0.1.0",
  "phase": 1,
  "completedTasks": ["Project scaffold", "Design document"],
  "currentTasks": [],
  "nextTasks": ["Create project scaffold", "Implement tile rendering"],
  "knownBugs": [],
  "decisions": [
    {
      "date": "2026-05-10",
      "decision": "Use Entity-Component composition pattern",
      "rationale": "Lightweight, no external dependency, sufficient for scope"
    }
  ],
  "dataFiles": ["classes.json", "enemies.json", "items.json", "potions.json", "scrolls.json", "tiles.json", "loot_tables.json", "special_attacks.json"],
  "lastAgent": null,
  "notes": ""
}
```

---

*This design document is a living artifact. Update as the project evolves.*
