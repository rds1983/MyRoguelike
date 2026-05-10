# MyRoguelike — Development Roadmap

> Detailed phase-by-phase task breakdown. Each phase ends with documentation updates.

---

## Phase 1: Project Scaffold & Core Infrastructure

**Goal:** Set up the .NET 8.0 + MonoGame project, establish core project structure, and prove the engine boots.

### Tasks
- [ ] 1.1 Install MonoGame templates and create the project
  - `dotnet new mgdesktopgl -o MyRoguelike` or equivalent
  - Target `net8.0`, verify `MyRoguelike.csproj`
- [ ] 1.2 Create directory structure per `docs/design.md` Section 2
  - All `src/` subdirectories, `Data/`, `Content/`, `Saves/`
- [ ] 1.3 Implement `Program.cs` entry point
- [ ] 1.4 Implement `Game1.cs` skeleton
  - `Initialize()`, `LoadContent()`, `Update()`, `Draw()`
  - Clear screen with a background color to confirm rendering works
- [ ] 1.5 Implement `Constants.cs`
  - Tile size (32×32), screen dimensions (1280×720), game title
- [ ] 1.6 Implement `Rng.cs`
  - Seeded `System.Random` wrapper with `Next()`, `Next(min, max)`, `NextDouble()`
- [ ] 1.7 Create placeholder MonoGame Content project (`Content/Content.mgcb`)
  - Add a single placeholder 32×32 PNG tile texture
- [ ] 1.8 Verify build and run
  - `dotnet build` succeeds, window opens and renders

### End-of-Phase Deliverables
- [ ] Update `User Guide.md` — Empty, note "Not yet applicable"
- [ ] Update `Developer Guide.md` — Section: "Build & Run", project structure overview
- [ ] Update `state.json` — Set `phase: 1`, mark Phase 1 tasks complete

---

## Phase 2: Data System (JSON Loading)

**Goal:** Load all game data from JSON files into typed C# models. No game logic yet.

### Tasks
- [ ] 2.1 Create all JSON data files in `Data/`
  - `classes.json` — 5 class definitions
  - `enemies.json` — initial 10 enemies (Tier 1)
  - `items.json` — initial 10 items
  - `potions.json` — initial 5 potions
  - `scrolls.json` — initial 5 scrolls
  - `tiles.json` — tile definitions (grass, stone, water, wall, etc.)
  - `loot_tables.json` — initial 5 loot tables
  - `special_attacks.json` — initial 5 special attacks
- [ ] 2.2 Implement C# model classes in `src/Data/Models/`
  - `ClassDef.cs`, `EnemyDef.cs`, `ItemDef.cs`, `PotionDef.cs`
  - `ScrollDef.cs`, `TileDef.cs`, `LootTableDef.cs`, `SpecialAttackDef.cs`
  - `StatBlock.cs` — reusable STR/DEX/CON/INT/WIS struct
- [ ] 2.3 Implement `DataManager.cs`
  - `LoadAll()` method that reads and deserializes all JSON files
  - Generic `Load<T>(string path)` helper
  - `GetClass(string id)`, `GetEnemy(string id)`, `GetItem(string id)`, etc.
  - Dictionaries keyed by `id` for O(1) lookup
- [ ] 2.4 Implement custom `JsonConverter` classes in `src/Data/Converters/` if needed
  - e.g., `StatBlockConverter`, `RangeConverter` for min/max
- [ ] 2.5 Wire `DataManager.LoadAll()` into `Game1.Initialize()`
- [ ] 2.6 Write a unit-test-able validation
  - Log warnings for missing references (e.g., enemy references `lootTable: "goblin"` but no such table)
- [ ] 2.7 Verify: all JSON loads without errors at startup

### End-of-Phase Deliverables
- [ ] Update `Developer Guide.md` — Section: "Data System", JSON schema reference
- [ ] Update `state.json` — Set `phase: 2`, mark tasks complete

---

## Phase 3: Entity System & Components

**Goal:** Implement the base entity and component model. Entities are composed of reusable components.

### Tasks
- [ ] 3.1 Implement `Entity.cs` base class
  - `Id`, `Name`, `Position (Point)`, `Components: List<IComponent>`
  - `AddComponent<T>()`, `GetComponent<T>()`, `HasComponent<T>()`
- [ ] 3.2 Implement `IComponent` interface (marker)
- [ ] 3.3 Implement `StatsComponent.cs`
  - Current HP/MP, Max HP/MP, STR/DEX/CON/INT/WIS
  - `ApplyDamage()`, `Heal()`, `RestoreMana()`, `IsAlive` property
- [ ] 3.4 Implement `InventoryComponent.cs`
  - `List<Item>` items, capacity limit
  - `AddItem()`, `RemoveItem()`, `HasItem(string id)`
- [ ] 3.5 Implement `EquipmentComponent.cs`
  - Slots: Weapon, Armor, Shield, Accessory1, Accessory2
  - `Equip()`, `Unequip()`, `GetEquipmentBonuses()` → `StatBlock`
- [ ] 3.6 Implement `CombatComponent.cs`
  - `AttackPower`, `Defense`, `Accuracy`, `Evasion`, `CritChance`
  - Calculated from base stats + equipment bonuses
- [ ] 3.7 Implement `AiComponent.cs`
  - `BehaviorType` (aggressive, defensive, cowardly, boss)
  - `CurrentState` (idle, alert, attacking, fleeing, patrolling)
- [ ] 3.8 Implement `EffectComponent.cs`
  - `List<ActiveEffect>` — each has `effectId`, `duration`, `source`
  - `ApplyEffect()`, `TickEffects()`, `RemoveExpired()`
- [ ] 3.9 Implement `Player.cs` subclass
  - `PlayerClass`, `Experience`, `Level`
  - `AddXp()`, `LevelUp()`, `StatPointsAvailable`
- [ ] 3.10 Implement `Enemy.cs` subclass
  - `EnemyDef` reference, `LootDrop()` method
- [ ] 3.11 Implement `Npc.cs` subclass
  - `Dialogue` lines, `ShopInventory` for shopkeepers
- [ ] 3.12 Verify entity creation from JSON data (manual debug test)

### End-of-Phase Deliverables
- [ ] Update `Developer Guide.md` — Section: "Entity System & Components"
- [ ] Update `state.json` — Set `phase: 3`, mark tasks complete

---

## Phase 4: Rendering Engine & Tile Map

**Goal:** Render a tile-based map. The foundation for all scenes.

### Tasks
- [ ] 4.1 Implement `Tile.cs`
  - `TileType`, `Position`, `IsWalkable`, `IsTransparent`, `Glyph`/`TextureId`
- [ ] 4.2 Implement a basic `Map.cs`
  - 2D `Tile[,]` array, width/height
  - `GetTile(x,y)`, `SetTile(x,y,tile)`, `IsInBounds(x,y)`
- [ ] 4.3 Implement `Camera.cs`
  - Follows a target entity (player), clamps to map bounds
  - `WorldToScreen(Point)`, `ScreenToWorld(Point)` conversions
- [ ] 4.4 Create placeholder tile textures in Content project
  - Create colored 32×32 PNG placeholders (grass green, stone gray, water blue, wall dark gray)
  - Add them to `Content.mgcb`
- [ ] 4.5 Implement a test scene `OverworldScene.cs` (skeleton)
  - Inherits from abstract `Scene.cs`
  - Generates a small 40×30 test map with random tiles
  - Renders the tile map via `SpriteBatch`
- [ ] 4.6 Implement `SceneManager.cs`
  - Stack-based: `Push()`, `Pop()`, `Peek()`
  - Delegates `Update`/`Draw` to top scene
- [ ] 4.7 Wire `SceneManager` into `Game1`, push test scene
- [ ] 4.8 Verify: colored tiles render in a window

### End-of-Phase Deliverables
- [ ] Update `Developer Guide.md` — Section: "Rendering & Tile Maps"
- [ ] Update `state.json` — Set `phase: 4`, mark tasks complete

---

## Phase 5: Player Movement & Controls

**Goal:** Player entity exists on the map and moves with keyboard input.

### Tasks
- [ ] 5.1 Implement input handling in `OverworldScene.cs`
  - Read keyboard state in `Update()`
  - Map WASD/Arrow keys to movement directions
- [ ] 5.2 Implement player movement
  - Check `Tile.IsWalkable` before moving
  - Update `Entity.Position`
  - Move camera to follow player
- [ ] 5.3 Implement `TitleScene.cs`
  - "Press Enter to Start" text rendering
  - Uses `SpriteFont` from Content
- [ ] 5.4 Implement `GameOverScene.cs` (basic)
  - "You Died" text, "Press Enter to Restart"
- [ ] 5.5 Add collision with map edges
- [ ] 5.6 Add smooth camera scrolling (lerp)
- [ ] 5.7 Add stair / exit tiles: pressing Enter on them changes scene
- [ ] 5.8 Verify: player moves, camera follows, cannot walk through walls

### End-of-Phase Deliverables
- [ ] Update `User Guide.md` — Section: "Controls"
- [ ] Update `Developer Guide.md` — Section: "Input & Movement"
- [ ] Update `state.json` — Set `phase: 5`, mark tasks complete

---

## Phase 6: Combat System

**Goal:** Turn-based combat between player and enemies. Hit/miss, damage, death.

### Tasks
- [ ] 6.1 Implement `CombatSystem.cs`
  - `CalculateHit(attacker, defender)` → hit/miss based on accuracy vs evasion
  - `CalculateDamage(attacker, defender)` → damage = ATK - DEF (min 1)
  - `CalculateCrit(attacker)` → double damage on crit
  - `ApplyDamage(target, amount)` → reduces HP, triggers events
- [ ] 6.2 Implement `TurnSystem.cs`
  - `TurnQueue` sorted by entity speed (DEX + equipment bonuses)
  - `NextTurn()` advances to next entity in queue
  - Each entity gets one action per turn
- [ ] 6.3 Implement basic enemy AI in `AiSystem.cs`
  - If player is adjacent → attack
  - If player is in detection range → move toward player
  - Otherwise → idle / patrol
- [ ] 6.4 Add combat controls for player
  - `Shift + direction` = melee attack in direction
  - Display damage numbers as floating text
- [ ] 6.5 Implement entity death
  - Remove dead enemies from the map
  - Grant XP to player
  - Drop loot (instant pickup or on ground)
- [ ] 6.6 Implement `EventSystem.cs`
  - Game events: `EntityKilled`, `EntityDamaged`, `LevelUp`, `ItemPickedUp`
  - Other systems subscribe to events
- [ ] 6.7 Implement `MessageLog.cs`
  - Scrolling log of combat messages
  - Color-coded: red (damage taken), white (damage dealt), green (heal), yellow (loot)
- [ ] 6.8 Implement `PathfindingSystem.cs`
  - A* algorithm for grid-based maps
  - Used by AI to navigate to player
- [ ] 6.9 Verify: player attacks enemies, enemies fight back, death works

### End-of-Phase Deliverables
- [ ] Update `User Guide.md` — Section: "Combat"
- [ ] Update `Developer Guide.md` — Section: "Combat System"
- [ ] Update `state.json` — Set `phase: 6`, mark tasks complete

---

## Phase 7: World Generator (Terrain)

**Goal:** Procedurally generate the overworld map with biomes.

### Tasks
- [ ] 7.1 Implement `WorldGenerator.cs`
  - Entry point: `Generate(int seed)` → returns `World`
- [ ] 7.2 Implement `BiomeGenerator.cs`
  - Multi-octave Perlin noise for heightmap
  - Height-to-biome mapping (ocean, coast, plains, forest, hills, mountains, swamp, tundra, desert)
  - Moisture noise layer for biome variation
- [ ] 7.3 Implement `World.cs`
  - Holds 2D tile map for overworld
  - `Seed`, `Width`, `Height`, `BiomeMap`, `Regions` list
- [ ] 7.4 Implement `Region.cs`
  - `Name`, `Type` (City, Village, Dungeon, Wild), `Position`, `Bounds`
  - City/Village: `Population`, `Shops`, `QuestsAvailable`
  - Dungeon: `Floors`, `BossDefId`, `MinLevel`, `MaxLevel`
- [ ] 7.5 Implement `NameGenerator.cs`
  - Syllable-based (consonant-vowel-consonant patterns)
  - Named entity lists for: cities, villages, dungeons, NPCs, gods
- [ ] 7.6 Generate a 100×100 overworld map with all biomes
- [ ] 7.7 Render the overworld in `OverworldScene.cs` (camera + tiles)
- [ ] 7.8 Verify: overworld renders with varied biomes, regions placed

### End-of-Phase Deliverables
- [ ] Update `Developer Guide.md` — Section: "World Generation"
- [ ] Update `state.json` — Set `phase: 7`, mark tasks complete

---

## Phase 8: Settlement Generation

**Goal:** Place cities, villages, and dungeons on the overworld.

### Tasks
- [ ] 8.1 Implement `CityGenerator.cs`
  - 3–5 cities placed near water / crossroads
  - City layout: walls, gates, buildings (shop, inn, temple, guild)
  - NPC population: shopkeepers, quest givers, citizens
- [ ] 8.2 Implement village generation
  - 10–20 villages in resource-rich biomes
  - Smaller layout: 3–5 buildings, 1 shop
- [ ] 8.3 Implement dungeon placement
  - 15–30 dungeons in mountains, forests, underground
  - Linked to world history (e.g., "Ruins of Old Thornhaven")
- [ ] 8.4 Add entry interaction
  - Player steps on city/village/dungeon tile → Enter prompt
  - Enter transitions to appropriate scene
- [ ] 8.5 Verify: settlements appear on overworld, player can enter them

### End-of-Phase Deliverables
- [ ] Update `Developer Guide.md` — Section: "Settlement Generation"
- [ ] Update `state.json` — Set `phase: 8`, mark tasks complete

---

## Phase 9: Dungeon Generator

**Goal:** Procedurally generate dungeon floors with rooms, corridors, and content.

### Tasks
- [ ] 9.1 Implement `DungeonGenerator.cs`
  - BSP (Binary Space Partitioning) algorithm for room placement
  - Corridor connection between rooms (A* or simple L-shaped)
- [ ] 9.2 Implement room types
  - Spawn room (stairs down entry)
  - Monster room (3–8 enemies)
  - Treasure room (chests with loot)
  - Trap room (pressure plates, dart traps, pit traps)
  - Shop room (NPC merchant on dungeon floor)
  - Boss room (final floor, contains boss enemy)
- [ ] 9.3 Implement `DungeonScene.cs`
  - Renders dungeon tiles (stone floor, walls, doors, stairs)
  - Populates enemies per room based on dungeon depth
  - Handles floor transitions (stairs down → next floor)
- [ ] 9.4 Implement `FovSystem.cs`
  - Ray-casting or recursive shadowcasting field-of-view
  - Tiles outside FOV are dark (not rendered or shown as black)
  - Previously seen tiles rendered dim (fog of war)
- [ ] 9.5 Implement lighting
  - Player light radius (torchlight, ~8 tiles)
  - Light-emitting items/tiles (braziers, glowing mushrooms)
- [ ] 9.6 Trap system
  - Player steps on trap → trigger effect (damage, status, teleport)
  - Detection via high WIS or trap-perception skill
- [ ] 9.7 Verify: dungeon generation, floor progression, combat, loot, traps

### End-of-Phase Deliverables
- [ ] Update `User Guide.md` — Section: "Dungeons"
- [ ] Update `Developer Guide.md` — Section: "Dungeon Generator", "FOV"
- [ ] Update `state.json` — Set `phase: 9`, mark tasks complete

---

## Phase 10: Items & Inventory

**Goal:** Full item system — pickup, use, equip, drop, with all 50+ items defined.

### Tasks
- [ ] 10.1 Complete all item definitions in `Data/items.json`
  - 20+ weapons, 12 armor, 5 shields, 8 accessories, 10 materials/quest
  - Each with stats, value, tier, description
- [ ] 10.2 Implement item pickup
  - Items drop on ground as entities
  - Walk over or press `G` to pick up
  - Message log: "Picked up Iron Sword"
- [ ] 10.3 Implement `InventoryScene.cs`
  - Grid or list view of inventory items
  - Select item → options: Use, Equip, Drop, Inspect
  - Filter by category tabs
- [ ] 10.4 Implement equipment system
  - `EquipmentComponent` equips weapon, armor, shield, accessories
  - Stat bonuses from equipped items apply to player
  - Visual feedback (character sheet updates)
- [ ] 10.5 Implement item rarity system
  - Common (white), Uncommon (green), Rare (blue), Epic (purple), Legendary (orange)
  - Color-coded item names in UI
- [ ] 10.6 Implement magic item affixes
  - Prefix/suffix modifiers (e.g., "Burning Iron Sword of Power")
  - Weighted random generation based on item tier
- [ ] 10.7 Implement `CharacterScene.cs`
  - Character sheet: stats, level, XP, class
  - Equipment slots shown visually
  - Stat comparison when hovering equipment
- [ ] 10.8 Implement `Tooltip.cs`
  - Hover over item → tooltip with name, stats, description, value
  - Position tooltip to avoid going off-screen
- [ ] 10.9 Verify: full inventory workflow — pickup, equip, unequip, drop, inspect

### End-of-Phase Deliverables
- [ ] Update `User Guide.md` — Section: "Items & Inventory", "Equipment"
- [ ] Update `Developer Guide.md` — Section: "Item System", "Rarity & Affixes"
- [ ] Update `state.json` — Set `phase: 10`, mark tasks complete

---

## Phase 11: Shops & Economy

**Goal:** Buy and sell items at NPC shopkeepers in cities and villages.

### Tasks
- [ ] 11.1 Implement `ShopScene.cs`
  - Two panels: shop inventory (left) and player inventory (right)
  - Buy: click shop item → costs gold → added to player inventory
  - Sell: click player item → 50% base price → gold added
- [ ] 11.2 Generate shop inventories
  - Based on settlement tier (village = basic, city = advanced)
  - Random selection from loot tables weighted by tier
  - Restock timer (new items after N days)
- [ ] 11.3 Implement shopkeeper NPC
  - Dialogue window with greeting
  - "Welcome, traveler. Care to see my wares?"
  - Options: Buy, Sell, Leave
- [ ] 11.4 Implement gold system
  - Player has `Gold` property on Player entity
  - Enemy drops gold (random range from EnemyDef)
  - Shop transactions modify gold
- [ ] 11.5 Add item identification
  - Some items are "Unidentified" — need Scroll of Identify or pay shopkeeper
  - Identified items reveal true stats/name
- [ ] 11.6 Verify: buy/sell flow works, gold updates, shop inventories vary

### End-of-Phase Deliverables
- [ ] Update `User Guide.md` — Section: "Shops & Economy"
- [ ] Update `Developer Guide.md` — Section: "Shop System"
- [ ] Update `state.json` — Set `phase: 11`, mark tasks complete

---

## Phase 12: Special Attacks, Potions & Scrolls

**Goal:** Implement consumables and unique enemy abilities.

### Tasks
- [ ] 12.1 Implement special attack system
  - `CombatComponent.UseSpecialAttack(string attackId, target)`
  - Cooldown tracking per entity
  - Special attack types: single_target, cone_aoe, radius_aoe, self_buff, debuff
- [ ] 12.2 Implement all special attacks from design doc
  - Dragon Breath, Death Roll, Petrifying Gaze, Soul Drain, Blood Drain
  - Siren Song, Disintegration Ray, Entangle, Possess, Multi-Head Strike
  - Web Trap, Ground Pound
- [ ] 12.3 Implement all potions in `src/Data/potions.json` + game logic
  - Health Potion, Mana Potion, Strength Potion, Speed Potion
  - Invisibility Potion, Antidote, Full Heal
  - `PotionSystem.cs` — `UsePotion(entity, potionDef)` applies effect
- [ ] 12.4 Implement all scrolls in `Data/scrolls.json` + game logic
  - Fireball, Teleportation, Identify, Enchantment
  - Summoning, Curse
  - `ScrollSystem.cs` — `UseScroll(entity, scrollDef)` invokes effect
- [ ] 12.5 Implement status effects
  - Poison, Burn, Freeze, Stun, Confuse, Root, Invisible, Bleed
  - Each with duration, tick damage/effect, visual indicator
- [ ] 12.6 Add hotbar / quick-slot system
  - Assign potions/scrolls to keys 1–9
  - Use with single keypress
- [ ] 12.7 Verify: special attacks trigger correctly, potions/scrolls work

### End-of-Phase Deliverables
- [ ] Update `User Guide.md` — Section: "Potions & Scrolls", "Special Abilities"
- [ ] Update `Developer Guide.md` — Section: "Special Attacks", "Consumables"
- [ ] Update `state.json` — Set `phase: 12`, mark tasks complete

---

## Phase 13: Overworld Travel & Encounters

**Goal:** Traverse the overworld, random encounters, fast travel, day/night cycle.

### Tasks
- [ ] 13.1 Implement overworld movement
  - Player moves on overworld tile map
  - Different movement costs per biome (plains = 1, forest = 2, mountain = 3, swamp = 4)
- [ ] 13.2 Implement random encounter system
  - Each step has a % chance of encounter based on biome + danger level
  - Higher at night, in dungeons, in high-level areas
  - Encounter triggers `CombatScene` with randomly selected enemies for the region
- [ ] 13.3 Implement fast travel
  - Discovered cities/villages can be fast-traveled to
  - Open map (M key) → click discovered settlement → travel
  - Costs a small amount of in-game time
- [ ] 13.4 Implement fog of war on overworld
  - Unexplored tiles are black/hidden
  - Explored tiles are dimmed
  - Current visible tiles are bright
- [ ] 13.5 Implement day/night cycle
  - Based on step count (or real time)
  - Visual: sky darkens at night, light radius shrinks
  - Encounters more frequent/dangerous at night
- [ ] 13.6 Verify: overworld travel, encounters, fast travel all functional

### End-of-Phase Deliverables
- [ ] Update `User Guide.md` — Section: "Overworld Travel"
- [ ] Update `Developer Guide.md` — Section: "Overworld System"
- [ ] Update `state.json` — Set `phase: 13`, mark tasks complete

---

## Phase 14: History & Mythology Generation

**Goal:** Procedurally generate world history and mythology à la Dwarf Fortress.

### Tasks
- [ ] 14.1 Implement `HistoryGenerator.cs`
  - Generate a timeline of events across eras
  - Each event: `Year`, `Type`, `Description`, `Actors[]`, `Locations[]`
  - Events create world state: ruined cities, named NPCs, artifacts
- [ ] 14.2 Implement history eras
  - **Creation Era (Year ~0–500):** Gods create the world, first races awaken
  - **Age of Myth (Year ~500–1500):** First empires rise, legendary beasts
  - **Age of Heroes (Year ~1500–2500):** Great wars, cataclysms, hero figures
  - **Recent Age (Year ~2500–3000):** Current world state, faction borders
  - **Player Arrival:** The player enters the world
- [ ] 14.3 Implement event types
  - `war`, `battle`, `plague`, `famine`, `disaster`, `founding`, `destruction`
  - `hero_birth`, `hero_death`, `artifact_creation`, `treaty`, `betrayal`
  - Each type has templates with parameterized names/locations
- [ ] 14.4 Implement `MythologyGenerator.cs`
  - Pantheon of 5–8 gods with procedurally generated names, domains, symbols
  - Creation myth narrative (parameterized text template)
  - Each god linked to historical events (e.g., "The War of the Burning Sky was caused by Solara's wrath")
- [ ] 14.5 Connect history to game content
  - Ruined cities from history appear as dungeon locations
  - Named artifacts from history spawn as legendary items
  - Historical figures' names used for NPCs, titles, locations
- [ ] 14.6 Add in-game lore browser
  - Menu tab: "Lore" — browse history timeline, god descriptions
  - Unlock lore entries as player discovers locations
- [ ] 14.7 Verify: history generates, mythology generates, connected to world

### End-of-Phase Deliverables
- [ ] Update `User Guide.md` — Section: "Lore & History"
- [ ] Update `Developer Guide.md` — Section: "History & Mythology Generation"
- [ ] Update `state.json` — Set `phase: 14`, mark tasks complete

---

## Phase 15: All 50+ Enemies

**Goal:** Define and implement all 50+ enemies with unique behaviors.

### Tasks
- [ ] 15.1 Complete `Data/enemies.json` with all 50+ enemies
  - Tier 1 (10): Goblin Scout, Giant Rat, Slime, Skeleton, Bat, Spider, Wolf, Bandit, Mushroom Man, Fire Sprite
  - Tier 2 (15): Orc Warrior, Dark Elf, Werewolf, Wraith, Harpy, Basilisk, Troll, Golem, Frost Spider, Shadow, Cultist, Treant, Manticore, Lizardman, Cave Ogre
  - Tier 3 (15): Vampire, Lich, Demon, Chimera, Hydra, Beholder, Fire Giant, Frost Giant, Storm Elemental, Dracolich, Mind Flayer, Death Knight, Succubus, Naga Queen, Abyssal Lord
  - Bosses (10): Dragon (fire/ice/void), Kraken, Titan, Archdemon, Eldritch Horror, Phoenix, Leviathan, Necromancer Lord, Colossus, Void Walker
- [ ] 15.2 Implement unique AI behaviors per enemy type
  - Aggressive: charge player on sight
  - Pack: fight in groups, flank
  - Cowardly: flee when low HP
  - Boss: phase transitions, enrage at 30% HP
  - Caster: stay at range, use special attacks
- [ ] 15.3 Assign special attacks to appropriate enemies (from Phase 12)
- [ ] 15.4 Implement enemy loot drops using `Data/loot_tables.json`
- [ ] 15.5 Verify: all 50+ enemies spawn, fight, drop loot, use specials

### End-of-Phase Deliverables
- [ ] Update `User Guide.md` — Section: "Bestiary" (list of enemies with lore)
- [ ] Update `Developer Guide.md` — Section: "Enemy AI", "Loot System"
- [ ] Update `state.json` — Set `phase: 15`, mark tasks complete

---

## Phase 16: All 50+ Items & Loot Balancing

**Goal:** Complete all items, balance loot tables, implement item generation.

### Tasks
- [ ] 16.1 Complete `Data/items.json` with all 55+ items
- [ ] 16.2 Implement item generation system
  - Generate random items based on tier + rarity
  - Apply magic affixes (prefix + suffix)
  - Weighted by dungeon depth / enemy level
- [ ] 16.3 Complete all loot tables in `Data/loot_tables.json`
  - Per-enemy tables (goblin, orc, dragon, etc.)
  - Per-location tables (dungeon chest, shop, boss)
  - Generic tables (junk, gems, potions)
- [ ] 16.4 Implement item stacking (potions, scrolls, materials)
  - Stack up to 20 of same item
- [ ] 16.5 Implement item destruction (optional)
  - Items on ground despawn after 100 turns
- [ ] 16.6 Balance pass on item stats
  - Ensure tier progression makes sense (Tier 1 items < Tier 2 < Tier 3)
  - Ensure gold values are balanced against enemy gold drops
- [ ] 16.7 Verify: item generation produces varied, balanced results

### End-of-Phase Deliverables
- [ ] Update `Developer Guide.md` — Section: "Item Generation & Balancing"
- [ ] Update `state.json` — Set `phase: 16`, mark tasks complete

---

## Phase 17: UI Polish & Menus

**Goal:** Full UI system — HUD, all menus, pause screen, options.

### Tasks
- [ ] 17.1 Implement `Hud.cs`
  - HP bar (red), MP bar (blue), XP bar (yellow)
  - Health/Mana numbers (current / max)
  - Level display
  - Gold counter
  - Active effects icons (top-right)
  - Minimap (top-right, small scale)
  - Current floor / location name
- [ ] 17.2 Implement all scene menus
  - `InventoryScene.cs` — tabbed views (items, equipment, quests)
  - `CharacterScene.cs` — stats, skills, equipment slots
  - `MapScene.cs` — full overworld map with FoW
  - `LogScene.cs` — full scrolling message history
- [ ] 17.3 Implement `Menu.cs` (pause menu)
  - Resume, Save, Load, Settings, Quit to Title
- [ ] 17.4 Implement settings
  - Volume sliders (master, SFX, music)
  - Text scroll speed
  - Fullscreen toggle
- [ ] 17.5 Add keyboard navigation for all menus
  - Arrow keys to navigate, Enter to select, Esc to go back
  - Selected item highlighted
- [ ] 17.6 Implement mouse support
  - Click to target enemies
  - Click to move (if tile is walkable)
  - Hover tooltips on items and entities
- [ ] 17.7 Add transition effects
  - Fade in/out between scenes
  - Screen shake on big damage
- [ ] 17.8 Verify: all menus functional, keyboard + mouse input work

### End-of-Phase Deliverables
- [ ] Update `User Guide.md` — Section: "UI & Menus", "Settings"
- [ ] Update `Developer Guide.md` — Section: "UI System"
- [ ] Update `state.json` — Set `phase: 17`, mark tasks complete

---

## Phase 18: Save / Load System

**Goal:** Full save/load with auto-save, multiple slots, error recovery.

### Tasks
- [ ] 18.1 Implement `SaveData.cs`
  - Serializable model: player state, world seed, explored tiles, inventory, equipment
  - quest progression, history timeline index, current scene, position
- [ ] 18.2 Implement `SaveManager.cs`
  - `Save(slotIndex)` — serializes to `Saves/world_{slot}.json`
  - `Load(slotIndex)` — deserializes and restores state
  - `ListSaves()` — returns save metadata (timestamp, level, location)
- [ ] 18.3 Implement auto-save triggers
  - Entering/leaving dungeon
  - Entering/leaving shop
  - Resting at inn
  - Level up
  - Interval-based (every 5 minutes)
- [ ] 18.4 Implement save slots UI
  - Save scene: 3 save slots with metadata display
  - Load scene: 3 save slots with preview (player name, level, play time)
- [ ] 18.5 Implement error handling
  - Corrupted save → recover from backup (`*.bak`)
  - Save corruption detection (checksum / hash)
- [ ] 18.6 Implement quit without saving warning
- [ ] 18.7 Verify: full save/load cycle works, auto-save triggers correctly

### End-of-Phase Deliverables
- [ ] Update `User Guide.md` — Section: "Save & Load"
- [ ] Update `Developer Guide.md` — Section: "Save System"
- [ ] Update `state.json` — Set `phase: 18`, mark tasks complete

---

## Phase 19: Sound & Particles

**Goal:** Audio feedback and visual particle effects.

### Tasks
- [ ] 19.1 Source or create placeholder sound effects
  - Hit, miss, crit, heal, magic cast, pickup, coin, door open, step
  - Death (player), death (enemy), level up, error
- [ ] 19.2 Add sounds to Content.mgcb and load via `ContentManager`
- [ ] 19.3 Implement `AudioSystem.cs` or integrate via EventSystem
  - `PlaySound(soundId)` plays once at given volume
  - `PlayMusic(trackId)` loops background music per scene
  - Volume control (master, SFX, music)
- [ ] 19.4 Add background music tracks
  - Title theme, overworld theme, dungeon theme, shop theme, boss theme
- [ ] 19.5 Implement `ParticleSystem.cs`
  - Particle: position, velocity, lifetime, color, size, alpha
  - Emitter: spawns particles over time or on event
- [ ] 19.6 Implement particle effects
  - Hit sparks, heal glow, magic swirl, fire, ice, poison cloud
  - Level up explosion, pickup sparkle, death explosion
  - Footsteps (dust)
- [ ] 19.7 Add screen shake on large damage / boss attacks
- [ ] 19.8 Verify: sounds play, particles render, no performance issues

### End-of-Phase Deliverables
- [ ] Update `User Guide.md` — Section: "Audio & Effects" (optional)
- [ ] Update `Developer Guide.md` — Section: "Audio System", "Particle System"
- [ ] Update `state.json` — Set `phase: 19`, mark tasks complete

---

## Phase 20: Balance Pass & Polish

**Goal:** Comprehensive balance pass across all game systems.

### Tasks
- [ ] 20.1 Balance combat numbers
  - Ensure Tier 1 enemies are beatable at level 1
  - Ensure Tier 3 enemies are challenging at level 30+
  - Bosses should require strategy (use consumables, positioning)
- [ ] 20.2 Balance item stats
  - Weapon damage progression: +2 per tier
  - Armor defense progression: +1 per tier
  - Gold values: enough to buy potions but not trivialize shops
- [ ] 20.3 Balance XP curve
  - Level 1→2: 100 XP, Level 49→50: 50000 XP
  - XP from enemies scales with tier
- [ ] 20.4 Balance encounter rates
  - Overworld: 8–15% per step (varies by biome)
  - Dungeon: 10–20% per step
  - Night: double encounter rate
- [ ] 20.5 Balance shop prices
  - Sell price: 50% of base value
  - Buy price: 100–150% of base value (random markup)
- [ ] 20.6 Balance potion/scroll effectiveness
  - Health potions restore 30/60/100 HP (small/medium/large)
  - Scroll damage scales with dungeon depth
- [ ] 20.7 Verify: playtest from start to mid-game, adjust numbers

### End-of-Phase Deliverables
- [ ] Update `Developer Guide.md` — Section: "Balance & Tuning"
- [ ] Update `state.json` — Set `phase: 20`, mark tasks complete

---

## Phase 21: User Guide

**Goal:** Write a comprehensive user-facing manual.

### Tasks
- [ ] 21.1 Write `User Guide.md` with full content
  - **Introduction:** What is MyRoguelike, story premise
  - **Installation:** How to build and run
  - **Getting Started:** Character creation, controls
  - **Classes:** Overview of all 5 classes
  - **Combat:** Turn order, actions, damage, death
  - **Items & Equipment:** Categories, rarity, affixes
  - **Potions & Scrolls:** Effects, how to use
  - **Overworld:** Travel, biomes, fast travel, encounters
  - **Dungeons:** Floor generation, traps, bosses
  - **Shops:** Buy, sell, identify
  - **World Lore:** History, mythology, how to discover
  - **Enemies / Bestiary:** Tier list, special attacks
  - **Character Progression:** Leveling, skills, stats
  - **UI & Controls:** Full keybind reference
  - **Save System:** Auto-save, manual save, slots
  - **Tips & Strategies:** Per-class play tips
  - **FAQ:** Common questions
- [ ] 21.2 Review and proofread
- [ ] 21.3 Create a `QUICKSTART.md` if desired (one-page cheat sheet)

### End-of-Phase Deliverables
- [ ] `User Guide.md` fully written and reviewed
- [ ] Update `state.json` — Set `phase: 21`, mark tasks complete

---

## Phase 22: Developer Guide

**Goal:** Write a comprehensive developer-oriented manual.

### Tasks
- [ ] 22.1 Write `Developer Guide.md` with full content
  - **Project Setup:** Prerequisites, clone, build, run
  - **Project Structure:** Every directory explained
  - **Architecture Overview:** High-level design patterns
  - **Data System:** JSON schema reference for all files
  - **Entity System:** How entities and components work
  - **Scene System:** Scene lifecycle, creating new scenes
  - **World Generation:** Algorithm details (Perlin noise, BSP, history)
  - **Combat System:** Damage formula, turn order, AI
  - **Item System:** Adding new items, loot tables, affixes
  - **Save System:** Save data format, adding new save fields
  - **UI System:** HUD, menus, custom UI components
  - **How to Add:** New class, new enemy, new item, new special attack
  - **Testing:** How to run and write tests
  - **Content Pipeline:** Working with .mgcb, adding sprites/sounds
  - **Performance:** Optimization tips
  - **Troubleshooting:** Common issues and fixes
- [ ] 22.2 Review and proofread

### End-of-Phase Deliverables
- [ ] `Developer Guide.md` fully written and reviewed
- [ ] Update `state.json` — Set `phase: 22`, mark tasks complete

---

## Phase 23: Final Integration & Release

**Goal:** Final testing, fixes, and `state.json` comprehensive update.

### Tasks
- [ ] 23.1 Full playthrough test (start to early/mid game)
- [ ] 23.2 Fix all known bugs from playthrough
- [ ] 23.3 Performance profiling and optimization
  - Reduce GC pressure, pool objects, optimize rendering
- [ ] 23.4 Edge case testing
  - Empty inventory, 0 HP edge, level cap, all classes
- [ ] 23.5 Final `state.json` update
  - Comprehensive snapshot of entire project
  - All completed tasks marked, all decisions logged
  - Known bugs and limitations documented
- [ ] 23.6 Create release build
  - `dotnet publish -c Release -r win-x64 --self-contained`
  - Package into distributable zip
- [ ] 23.7 Write release notes

### End-of-Phase Deliverables
- [ ] Release build artifact
- [ ] `state.json` comprehensive and up-to-date
- [ ] Release notes

---

## Phase Tracking

| Phase | Description | Est. Tasks | Status |
|-------|-------------|-----------|--------|
| 1 | Project Scaffold & Core Infrastructure | 8 | ❌ |
| 2 | Data System (JSON Loading) | 7 | ❌ |
| 3 | Entity System & Components | 12 | ❌ |
| 4 | Rendering Engine & Tile Map | 8 | ❌ |
| 5 | Player Movement & Controls | 8 | ❌ |
| 6 | Combat System | 9 | ❌ |
| 7 | World Generator (Terrain) | 8 | ❌ |
| 8 | Settlement Generation | 5 | ❌ |
| 9 | Dungeon Generator | 7 | ❌ |
| 10 | Items & Inventory | 9 | ❌ |
| 11 | Shops & Economy | 6 | ❌ |
| 12 | Special Attacks, Potions & Scrolls | 7 | ❌ |
| 13 | Overworld Travel & Encounters | 6 | ❌ |
| 14 | History & Mythology Generation | 7 | ❌ |
| 15 | All 50+ Enemies | 5 | ❌ |
| 16 | All 50+ Items & Loot Balancing | 7 | ❌ |
| 17 | UI Polish & Menus | 8 | ❌ |
| 18 | Save / Load System | 7 | ❌ |
| 19 | Sound & Particles | 8 | ❌ |
| 20 | Balance Pass & Polish | 7 | ❌ |
| 21 | User Guide | 3 | ❌ |
| 22 | Developer Guide | 2 | ❌ |
| 23 | Final Integration & Release | 7 | ❌ |
| **Total** | | **167** | |

---

## Documentation Update Checklist (End of Every Phase)

- [ ] `User Guide.md` — updated if any user-facing feature was added
- [ ] `Developer Guide.md` — updated with new system description
- [ ] `state.json` — phase number incremented, tasks marked complete, decisions logged

---

*This roadmap is a living document. Update tasks as the project evolves.*
