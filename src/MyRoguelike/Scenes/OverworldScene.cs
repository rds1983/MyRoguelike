using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MyRoguelike.Data.Models;
using MyRoguelike.Entities;
using MyRoguelike.Systems;
using MyRoguelike.UI;
using MyRoguelike.World;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace MyRoguelike.Scenes;

public class OverworldScene : Scene
{
    private Map _map = null!;
    private readonly Camera _camera;
    private readonly TurnSystem _turnSystem = new();
    private readonly MessageLog _messageLog = new();
    private Texture2D _tileTexture = null!;
    private Player _player = null!;
    private readonly List<Enemy> _enemies = [];
    private readonly Dictionary<(int x, int y), List<Item>> _groundItems = new();

    private Texture2D _messageBg = null!;
    private bool _gameOver;
    private World.World _world = null!;

    private KeyboardState _prevKeyboard;
    private bool _inventoryOpen;
    private int _inventoryIndex;
    private Texture2D _panelBg = null!;

    public OverworldScene()
    {
        _camera = new Camera(Constants.ScreenWidth, Constants.ScreenHeight);
    }

    public override void LoadContent()
    {
        _tileTexture = Game1.PlaceholderTile;

        var generator = new WorldGenerator();
        _world = generator.Generate(Constants.OverworldWidth, Constants.OverworldHeight);
        _map = _world.Map;

        _camera.SetMapBounds(_map.Width, _map.Height);

        var spawn = generator.FindPlayerSpawn(_world);
        _player = new Player
        {
            Id = "player",
            Name = "Hero",
            Position = spawn,
            Glyph = "@",
            Color = Color.White,
            ClassId = "warrior"
        };

        SetupPlayerComponents();
        SpawnEnemies(10);

        _turnSystem.Clear();
        _turnSystem.AddEntity(_player, GetEntitySpeed(_player));
        foreach (var enemy in _enemies)
            _turnSystem.AddEntity(enemy, GetEntitySpeed(enemy));

        _camera.CenterOn(_player.Position.X, _player.Position.Y);

        _messageBg = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
        _messageBg.SetData([new Color(0, 0, 0, 180)]);

        _panelBg = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
        _panelBg.SetData([new Color(0, 0, 0, 220)]);

        _messageLog.Add("The adventure begins...", Color.Gold);
        _messageLog.Add("Use WASD/Arrows to move or attack enemies in your path.", Color.Gray);
        _messageLog.Add("Press Space or . to wait a turn.", Color.Gray);
        _messageLog.Add("Press G to pick up items. Press I to open inventory.", Color.Gray);

        EventSystem.EntityKilled += OnEntityKilled;
        EventSystem.EntityDamaged += OnEntityDamaged;

        SeedStarterItems();
    }

    public override void Update(GameTime gameTime)
    {
        if (_gameOver) return;

        var kb = Keyboard.GetState();

        if (_turnSystem.CurrentActor is Player)
            HandlePlayerTurn(kb);

        while (_turnSystem.CurrentActor is Enemy enemy && !_gameOver)
            ProcessEnemyTurn(enemy);

        _prevKeyboard = kb;
    }

    private void HandlePlayerTurn(KeyboardState kb)
    {
        if (_inventoryOpen)
        {
            HandleInventoryInput(kb);
            return;
        }

        var acted = false;
        var dx = 0;
        var dy = 0;

        if ((kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.Up)) &&
            _prevKeyboard.IsKeyUp(Keys.W) && _prevKeyboard.IsKeyUp(Keys.Up)) dy = -1;
        else if ((kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Down)) &&
                 _prevKeyboard.IsKeyUp(Keys.S) && _prevKeyboard.IsKeyUp(Keys.Down)) dy = 1;
        else if ((kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.Left)) &&
                 _prevKeyboard.IsKeyUp(Keys.A) && _prevKeyboard.IsKeyUp(Keys.Left)) dx = -1;
        else if ((kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Right)) &&
                 _prevKeyboard.IsKeyUp(Keys.D) && _prevKeyboard.IsKeyUp(Keys.Right)) dx = 1;

        if (dx != 0 || dy != 0)
        {
            var targetX = _player.Position.X + dx;
            var targetY = _player.Position.Y + dy;

            var enemy = _enemies.FirstOrDefault(e =>
                e.Position.X == targetX && e.Position.Y == targetY &&
                e.GetComponent<StatsComponent>()?.IsAlive == true);

            if (enemy != null)
            {
                var result = CombatSystem.MeleeAttack(_player, enemy);
                _messageLog.Add(result.Message, Color.White);
            }
            else if (_map.IsWalkable(targetX, targetY))
            {
                _player.Position = new Point(targetX, targetY);
            }

            acted = true;
        }

        if (!acted && (kb.IsKeyDown(Keys.Space) || kb.IsKeyDown(Keys.OemPeriod)) &&
            _prevKeyboard.IsKeyUp(Keys.Space) && _prevKeyboard.IsKeyUp(Keys.OemPeriod))
        {
            acted = true;
        }

        if (!acted && kb.IsKeyDown(Keys.G) && _prevKeyboard.IsKeyUp(Keys.G))
        {
            acted = TryPickupHere();
        }

        if (!acted && kb.IsKeyDown(Keys.I) && _prevKeyboard.IsKeyUp(Keys.I))
        {
            _inventoryOpen = true;
        }

        if (!acted && kb.IsKeyDown(Keys.Enter) && _prevKeyboard.IsKeyUp(Keys.Enter))
        {
            TryInteract();
            acted = true;
        }

        if (acted)
        {
            _camera.CenterOn(_player.Position.X, _player.Position.Y);
            EffectSystem.Tick(_player);
            RecalculatePlayerCombat();
            _turnSystem.NextTurn();
        }
    }

    private void ProcessEnemyTurn(Enemy enemy)
    {
        var playerStats = _player.GetComponent<StatsComponent>();
        if (playerStats?.IsAlive != true)
        {
            _gameOver = true;
            Game1.Instance.SceneManager.Push(new GameOverScene());
            return;
        }

        var action = AiSystem.GetAction(enemy, _player, _map, point =>
            _enemies.Any(e => e.Position == point && e.GetComponent<StatsComponent>()?.IsAlive == true) ||
            _player.Position == point);

        switch (action.Type)
        {
            case AiActionType.MeleeAttack when action.Target is Player:
            {
                var result = CombatSystem.MeleeAttack(enemy, _player);
                _messageLog.Add(result.Message, Color.Red);

                if (result.TargetKilled)
                {
                    _gameOver = true;
                    Game1.Instance.SceneManager.Push(new GameOverScene());
                }

                break;
            }
            case AiActionType.Move:
                enemy.Position = action.TargetPosition;
                break;
        }

        _turnSystem.NextTurn();
    }

    private void OnEntityKilled(Entity killed, Entity? killer)
    {
        if (killed is Enemy enemy && killer == _player)
        {
            var stats = enemy.GetComponent<StatsComponent>();
            var xp = enemy.GetDef()?.XpReward ?? 0;
            var gold = enemy.GenerateGold();
            var loot = enemy.GenerateLoot();

            _player.AddXp(xp);

            _messageLog.Add($"Gained {xp} experience.", Color.Cyan);
            if (gold > 0)
            {
                _player.Gold += gold;
                _messageLog.Add($"Found {gold} gold.", Color.Gold);
            }

            foreach (var item in loot)
            {
                item.UpdateStackable();
                if (_player.GetComponent<InventoryComponent>()?.AddItem(item) == true)
                {
                    _messageLog.Add($"Picked up {item.DisplayName}.", Color.LightGreen);
                }
                else
                {
                    DropToGround(enemy.Position, item);
                    _messageLog.Add($"{item.DisplayName} drops to the ground.", Color.Gray);
                }
            }

            _map.SetTile(enemy.Position.X, enemy.Position.Y, "grass");
            _enemies.Remove(enemy);
            _turnSystem.RemoveEntity(enemy);
        }
    }

    private static void OnEntityDamaged(Entity damaged, Entity? attacker, int damage)
    {
    }

    private void SetupPlayerComponents()
    {
        var classDef = _player.GetClassDef();
        if (classDef == null) return;

        var stats = _player.AddComponent<StatsComponent>();
        stats.BaseStats = classDef.BaseStats;
        stats.SetHp(classDef.BaseStats.Constitution * 10 + classDef.HpPerLevel);
        stats.SetMp(classDef.BaseStats.Intelligence * 5 + classDef.MpPerLevel);

        var combat = _player.AddComponent<CombatComponent>();
        var equip = _player.AddComponent<EquipmentComponent>();
        _player.AddComponent<InventoryComponent>();
        _player.AddComponent<EffectComponent>();

        if (classDef.StartingEquipment != null)
        {
            if (!string.IsNullOrEmpty(classDef.StartingEquipment.Weapon) &&
                classDef.StartingEquipment.Weapon != "unarmed")
            {
                var weaponDef = Game1.Data.GetItem(classDef.StartingEquipment.Weapon);
                if (weaponDef != null)
                    equip.Equip(new Item { Id = weaponDef.Id, Def = weaponDef });
            }

            if (!string.IsNullOrEmpty(classDef.StartingEquipment.Armor))
            {
                var armorDef = Game1.Data.GetItem(classDef.StartingEquipment.Armor);
                if (armorDef != null)
                    equip.Equip(new Item { Id = armorDef.Id, Def = armorDef });
            }

            if (!string.IsNullOrEmpty(classDef.StartingEquipment.Shield))
            {
                var shieldDef = Game1.Data.GetItem(classDef.StartingEquipment.Shield);
                if (shieldDef != null)
                    equip.Equip(new Item { Id = shieldDef.Id, Def = shieldDef });
            }
        }

        combat.Recalculate(stats, equip);
    }

    private void SpawnEnemies(int count)
    {
        _enemies.Clear();
        var rng = new Random(_world.Seed + 999);

        var enemyDefIds = new[] { "goblin_scout", "giant_rat", "skeleton", "spider", "slime" };

        for (var i = 0; i < count; i++)
        {
            for (var attempt = 0; attempt < 50; attempt++)
            {
                var x = rng.Next(5, _map.Width - 5);
                var y = rng.Next(5, _map.Height - 5);

                if (!_map.IsWalkable(x, y)) continue;
                if (_enemies.Any(e => e.Position.X == x && e.Position.Y == y)) continue;
                var dist = Math.Abs(x - _player.Position.X) + Math.Abs(y - _player.Position.Y);
                if (dist < 10) continue;

                var defId = enemyDefIds[rng.Next(enemyDefIds.Length)];
                AddEnemy(defId, x, y);
                break;
            }
        }
    }

    private void AddEnemy(string enemyDefId, int x, int y)
    {
        var def = Game1.Data.GetEnemy(enemyDefId);
        if (def == null) return;

        var enemy = new Enemy
        {
            Id = $"enemy_{Guid.NewGuid():N}",
            Name = def.Name,
            Position = new Point(x, y),
            Glyph = def.Name.Length > 0 ? def.Name[0].ToString().ToLower() : "e",
            Color = Color.Red,
            EnemyDefId = enemyDefId
        };

        var stats = enemy.AddComponent<StatsComponent>();
        stats.BaseStats = def.Stats;
        stats.SetHp(def.Hp);
        stats.SetMp(def.Mp);

        var combat = enemy.AddComponent<CombatComponent>();
        combat.Recalculate(stats, new EquipmentComponent());

        enemy.AddComponent<AiComponent>();
        enemy.AddComponent<InventoryComponent>();

        _enemies.Add(enemy);
    }

    private static int GetEntitySpeed(Entity entity)
    {
        var stats = entity.GetComponent<StatsComponent>();
        return stats?.TotalDexterity ?? 10;
    }

    private void TryInteract()
    {
        var tile = _map.GetTile(_player.Position.X, _player.Position.Y);
        if (tile.TileDefId == "stairs_down")
        {
            var dungeonRegion = _world.Regions.FirstOrDefault(r =>
                r.Type == RegionType.Dungeon && r.Contains(_player.Position.X, _player.Position.Y));

            var dungeonSeed = _world.Seed + 20000 + (dungeonRegion?.CenterX ?? 0) * 31 + (dungeonRegion?.CenterY ?? 0) * 17;
            Game1.Instance.SceneManager.Push(new DungeonScene(_player, _player.Position, dungeonSeed));
        }
        else if (tile.TileDefId == "stairs_up")
        {
            _messageLog.Add("These stairs lead nowhere.", Color.Gray);
        }
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        var device = spriteBatch.GraphicsDevice;
        if (device == null) return;

        device.Clear(Color.Black);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        var font = Game1.Font;
        for (var x = 0; x < _map.Width; x++)
        for (var y = 0; y < _map.Height; y++)
        {
            if (!_camera.IsTileVisible(x, y)) continue;

            var tile = _map.GetTile(x, y);
            var screenRect = _camera.GetTileScreenRect(x, y);
            var color = tile.GetColor() ?? Color.White;

            spriteBatch.Draw(_tileTexture, screenRect, color * 0.15f);

            var glyph = tile.Glyph;
            var glyphSize = font.MeasureString(glyph);
            var pos = new Vector2(
                screenRect.X + (screenRect.Width - glyphSize.X) / 2f,
                screenRect.Y + (screenRect.Height - glyphSize.Y) / 2f
            );
            spriteBatch.DrawString(font, glyph, pos, color);
        }

        foreach (var enemy in _enemies)
        {
            if (!_camera.IsTileVisible(enemy.Position.X, enemy.Position.Y)) continue;
            if (enemy.GetComponent<StatsComponent>()?.IsAlive == false) continue;

            var screenRect = _camera.GetTileScreenRect(enemy.Position.X, enemy.Position.Y);
            var glyphSize = font.MeasureString(enemy.Glyph);
            var pos = new Vector2(
                screenRect.X + (screenRect.Width - glyphSize.X) / 2f,
                screenRect.Y + (screenRect.Height - glyphSize.Y) / 2f
            );
            spriteBatch.DrawString(font, enemy.Glyph, pos, enemy.Color);
        }

        if (_camera.IsTileVisible(_player.Position.X, _player.Position.Y))
        {
            var screenRect = _camera.GetTileScreenRect(_player.Position.X, _player.Position.Y);
            var glyphSize = font.MeasureString(_player.Glyph);
            var pos = new Vector2(
                screenRect.X + (screenRect.Width - glyphSize.X) / 2f,
                screenRect.Y + (screenRect.Height - glyphSize.Y) / 2f
            );
            spriteBatch.DrawString(font, _player.Glyph, pos, _player.Color);
        }

        spriteBatch.End();

        spriteBatch.Begin();
        spriteBatch.Draw(_messageBg, new Rectangle(0, Constants.ScreenHeight - 120,
            Constants.ScreenWidth, 120), Color.White);

        _messageLog.Draw(spriteBatch, font, 8, Constants.ScreenHeight - 115, 5, 22);

        if (_inventoryOpen)
            DrawInventoryOverlay(spriteBatch, font);

        spriteBatch.End();
    }

    private void SeedStarterItems()
    {
        // Place a few consumables near the spawn so Phase 10 is discoverable immediately.
        var positions = new[]
        {
            new Point(_player.Position.X + 1, _player.Position.Y),
            new Point(_player.Position.X, _player.Position.Y + 1),
            new Point(_player.Position.X + 1, _player.Position.Y + 1)
        };

        var ids = new[] { "health_potion", "mana_potion", "scroll_teleportation" };

        for (var i = 0; i < positions.Length; i++)
        {
            var p = positions[i];
            if (!_map.IsInBounds(p.X, p.Y) || !_map.IsWalkable(p.X, p.Y)) continue;
            var item = ItemFactory.Create(ids[i]);
            if (item != null) DropToGround(p, item);
        }
    }

    private void DropToGround(Point pos, Item item)
    {
        var key = (pos.X, pos.Y);
        if (!_groundItems.TryGetValue(key, out var list))
        {
            list = [];
            _groundItems[key] = list;
        }
        list.Add(item);
    }

    private bool TryPickupHere()
    {
        var inv = _player.GetComponent<InventoryComponent>();
        if (inv == null) return false;

        var key = (_player.Position.X, _player.Position.Y);
        if (!_groundItems.TryGetValue(key, out var list) || list.Count == 0)
        {
            _messageLog.Add("Nothing here to pick up.", Color.Gray);
            return false;
        }

        var pickedAny = false;
        while (list.Count > 0 && !inv.IsFull)
        {
            var item = list[0];
            list.RemoveAt(0);
            if (inv.AddItem(item))
            {
                _messageLog.Add($"Picked up {item.DisplayName}.", Color.LightGreen);
                pickedAny = true;
            }
        }

        if (list.Count == 0)
            _groundItems.Remove(key);

        if (!pickedAny)
            _messageLog.Add("Your inventory is full.", Color.Gray);

        return pickedAny;
    }

    private void HandleInventoryInput(KeyboardState kb)
    {
        var inv = _player.GetComponent<InventoryComponent>();
        var equip = _player.GetComponent<EquipmentComponent>();
        if (inv == null || equip == null)
        {
            _inventoryOpen = false;
            return;
        }

        if (kb.IsKeyDown(Keys.Escape) && _prevKeyboard.IsKeyUp(Keys.Escape))
        {
            _inventoryOpen = false;
            return;
        }

        if (kb.IsKeyDown(Keys.I) && _prevKeyboard.IsKeyUp(Keys.I))
        {
            _inventoryOpen = false;
            return;
        }

        if (kb.IsKeyDown(Keys.Up) && _prevKeyboard.IsKeyUp(Keys.Up))
            _inventoryIndex = Math.Max(0, _inventoryIndex - 1);
        else if (kb.IsKeyDown(Keys.Down) && _prevKeyboard.IsKeyUp(Keys.Down))
            _inventoryIndex = Math.Min(Math.Max(0, inv.Items.Count - 1), _inventoryIndex + 1);

        if (inv.Items.Count == 0) return;
        _inventoryIndex = Math.Clamp(_inventoryIndex, 0, inv.Items.Count - 1);
        var item = inv.Items[_inventoryIndex];

        if (kb.IsKeyDown(Keys.E) && _prevKeyboard.IsKeyUp(Keys.E))
        {
            if (equip.IsEquipped(item))
            {
                equip.Unequip(item);
                _messageLog.Add($"Unequipped {item.DisplayName}.", Color.Gray);
                RecalculatePlayerCombat();
                _inventoryOpen = false;
                ConsumePlayerTurn();
            }
            else if (item.Def?.Category is "weapon" or "armor" or "shield" or "accessory")
            {
                equip.Equip(item);
                _messageLog.Add($"Equipped {item.DisplayName}.", Color.LightGreen);
                RecalculatePlayerCombat();
                _inventoryOpen = false;
                ConsumePlayerTurn();
            }
            else
            {
                _messageLog.Add("That can't be equipped.", Color.Gray);
            }
        }

        if (kb.IsKeyDown(Keys.U) && _prevKeyboard.IsKeyUp(Keys.U))
        {
            if (ItemUseSystem.TryUse(_player, item, _map, _enemies, out var msg))
            {
                _messageLog.Add(msg, Color.LightGreen);
                inv.RemoveItem(item.Id, 1);
                RecalculatePlayerCombat();
                _inventoryOpen = false;
                ConsumePlayerTurn();
            }
            else
            {
                _messageLog.Add(msg, Color.Gray);
            }
        }

        if (kb.IsKeyDown(Keys.D) && _prevKeyboard.IsKeyUp(Keys.D))
        {
            if (equip.IsEquipped(item))
                equip.Unequip(item);

            var drop = ItemFactory.Create(item.Id, 1, item.IsIdentified);
            if (drop != null)
                DropToGround(_player.Position, drop);

            inv.RemoveItem(item.Id, 1);
            _messageLog.Add($"Dropped {item.DisplayName}.", Color.Gray);
            RecalculatePlayerCombat();
            _inventoryOpen = false;
            ConsumePlayerTurn();

            _inventoryIndex = Math.Clamp(_inventoryIndex, 0, Math.Max(0, inv.Items.Count - 1));
        }
    }

    private void DrawInventoryOverlay(SpriteBatch spriteBatch, SpriteFont font)
    {
        var inv = _player.GetComponent<InventoryComponent>();
        var equip = _player.GetComponent<EquipmentComponent>();
        if (inv == null || equip == null) return;

        var panelW = 520;
        var panelH = 420;
        var x = Constants.ScreenWidth - panelW - 20;
        var y = 20;
        spriteBatch.Draw(_panelBg, new Rectangle(x, y, panelW, panelH), Color.White);

        spriteBatch.DrawString(font, "Inventory", new Vector2(x + 14, y + 10), Color.Gold);
        spriteBatch.DrawString(font, "Up/Down: select  E: equip  U: use  D: drop  Esc/I: close",
            new Vector2(x + 14, y + 40), Color.Gray);

        var startY = y + 80;
        var maxLines = 14;
        var start = Math.Max(0, _inventoryIndex - maxLines / 2);
        var end = Math.Min(inv.Items.Count, start + maxLines);

        for (var i = start; i < end; i++)
        {
            var item = inv.Items[i];
            var selected = i == _inventoryIndex;

            var prefix = equip.IsEquipped(item) ? "[E] " : "    ";
            var qty = item.IsStackable ? $" x{item.Quantity}" : "";
            var line = $"{prefix}{item.DisplayName}{qty}";
            var color = selected ? Color.White : Color.LightGray;
            if (selected)
                spriteBatch.Draw(_panelBg, new Rectangle(x + 10, startY + (i - start) * 24 - 2, panelW - 20, 24), new Color(255, 255, 255, 30));
            spriteBatch.DrawString(font, line, new Vector2(x + 14, startY + (i - start) * 24), color);
        }

        if (inv.Items.Count == 0)
            spriteBatch.DrawString(font, "(empty)", new Vector2(x + 14, startY), Color.Gray);
    }

    private void RecalculatePlayerCombat()
    {
        var stats = _player.GetComponent<StatsComponent>();
        var combat = _player.GetComponent<CombatComponent>();
        var equip = _player.GetComponent<EquipmentComponent>();
        if (stats == null || combat == null || equip == null) return;
        combat.Recalculate(stats, equip);
    }

    private void ConsumePlayerTurn()
    {
        _camera.CenterOn(_player.Position.X, _player.Position.Y);
        EffectSystem.Tick(_player);
        RecalculatePlayerCombat();
        _turnSystem.NextTurn();
    }
}
