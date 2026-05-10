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
    private readonly Map _map;
    private readonly Camera _camera;
    private readonly TurnSystem _turnSystem = new();
    private readonly MessageLog _messageLog = new();
    private Texture2D _tileTexture = null!;
    private Player _player = null!;
    private readonly List<Enemy> _enemies = [];
    private Texture2D _messageBg = null!;
    private bool _gameOver;

    private const int TestMapWidth = 40;
    private const int TestMapHeight = 30;
    private KeyboardState _prevKeyboard;

    public OverworldScene()
    {
        _map = new Map(TestMapWidth, TestMapHeight);
        _camera = new Camera(Constants.ScreenWidth, Constants.ScreenHeight);
        _camera.SetMapBounds(TestMapWidth, TestMapHeight);
    }

    public override void LoadContent()
    {
        _tileTexture = Game1.PlaceholderTile;
        GenerateTestMap();

        _player = new Player
        {
            Id = "player",
            Name = "Hero",
            Position = new Point(TestMapWidth / 2, TestMapHeight / 2),
            Glyph = "@",
            Color = Color.White,
            ClassId = "warrior"
        };

        SetupPlayerComponents();
        SpawnEnemies();

        _turnSystem.Clear();
        _turnSystem.AddEntity(_player, GetEntitySpeed(_player));
        foreach (var enemy in _enemies)
            _turnSystem.AddEntity(enemy, GetEntitySpeed(enemy));

        _camera.CenterOn(_player.Position.X, _player.Position.Y);

        _messageBg = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
        _messageBg.SetData([new Color(0, 0, 0, 180)]);

        _messageLog.Add("The adventure begins...", Color.Gold);
        _messageLog.Add("Use WASD/Arrows to move or attack enemies in your path.", Color.Gray);
        _messageLog.Add("Press Space or . to wait a turn.", Color.Gray);

        EventSystem.EntityKilled += OnEntityKilled;
        EventSystem.EntityDamaged += OnEntityDamaged;
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
        var acted = false;
        var dx = 0;
        var dy = 0;

        if (kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.Up)) dy = -1;
        else if (kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Down)) dy = 1;
        else if (kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.Left)) dx = -1;
        else if (kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Right)) dx = 1;

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

        if (!acted && kb.IsKeyDown(Keys.Enter) && _prevKeyboard.IsKeyUp(Keys.Enter))
        {
            TryInteract();
            acted = true;
        }

        if (acted)
        {
            _camera.CenterOn(_player.Position.X, _player.Position.Y);
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
                if (_player.GetComponent<InventoryComponent>()?.AddItem(item) == true)
                    _messageLog.Add($"Picked up {item.DisplayName}.", Color.LightGreen);
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

    private void SpawnEnemies()
    {
        _enemies.Clear();

        AddEnemy("goblin_scout", 15, 10);
        AddEnemy("giant_rat", 25, 10);
        AddEnemy("skeleton", 8, 20);
        AddEnemy("spider", 30, 20);
        AddEnemy("slime", 18, 18);
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

    private void GenerateTestMap()
    {
        _map.Fill("grass");

        _map.FillRect(0, 0, TestMapWidth - 1, 0, "stone_wall");
        _map.FillRect(0, TestMapHeight - 1, TestMapWidth - 1, TestMapHeight - 1, "stone_wall");
        _map.FillRect(0, 0, 0, TestMapHeight - 1, "stone_wall");
        _map.FillRect(TestMapWidth - 1, 0, TestMapWidth - 1, TestMapHeight - 1, "stone_wall");

        for (var x = 10; x <= 16; x++)
        for (var y = 8; y <= 14; y++)
            _map.SetTile(x, y, "water");

        for (var x = 25; x <= 32; x++)
        for (var y = 18; y <= 24; y++)
            if ((x + y) % 3 != 0)
                _map.SetTile(x, y, "tree");

        for (var x = 5; x <= 20; x++)
            _map.SetTile(x, 20, "road");
        for (var y = 5; y <= 20; y++)
            _map.SetTile(20, y, "road");

        _map.SetTile(6, 6, "dirt");
        _map.SetTile(7, 6, "dirt");
        _map.SetTile(6, 7, "dirt");

        _map.SetTile(20, 5, "door");
        _map.SetTile(35, 25, "stairs_down");
        _map.SetTile(3, 3, "stairs_up");
    }

    private void TryInteract()
    {
        var tile = _map.GetTile(_player.Position.X, _player.Position.Y);
        if (tile.TileDefId == "stairs_down" || tile.TileDefId == "stairs_up")
        {
            var scene = new PlaceholderScene(
                tile.TileDefId == "stairs_down"
                    ? "You descend the stairs..."
                    : "You climb the stairs...");
            Game1.Instance.SceneManager.Push(scene);
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
        spriteBatch.End();
    }
}
