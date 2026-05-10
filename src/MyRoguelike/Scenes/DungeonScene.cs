using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MyRoguelike.Entities;
using MyRoguelike.Systems;
using MyRoguelike.UI;
using MyRoguelike.World;
using Color = Microsoft.Xna.Framework.Color;
using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace MyRoguelike.Scenes;

public sealed class DungeonScene : Scene
{
    private readonly Player _player;
    private readonly Point _returnPosition;
    private readonly int _seed;

    private Map _map = null!;
    private readonly Camera _camera;
    private Texture2D _tileTexture = null!;
    private Texture2D _messageBg = null!;
    private readonly MessageLog _messageLog = new();
    private readonly Dictionary<(int x, int y), List<Item>> _groundItems = new();

    private bool[,] _visible = null!;
    private bool[,] _explored = null!;

    private Point _stairsUp;
    private Point _stairsDown;

    private KeyboardState _prevKeyboard;
    private bool _inventoryOpen;
    private int _inventoryIndex;
    private Texture2D _panelBg = null!;

    public DungeonScene(Player player, Point returnPosition, int seed)
    {
        _player = player;
        _returnPosition = returnPosition;
        _seed = seed;
        _camera = new Camera(Constants.ScreenWidth, Constants.ScreenHeight);
    }

    public override void LoadContent()
    {
        _tileTexture = Game1.PlaceholderTile;

        var gen = new DungeonGenerator(_seed);
        var result = gen.Generate(80, 60);
        _map = result.Map;
        _stairsUp = result.StairsUp;
        _stairsDown = result.StairsDown;

        _camera.SetMapBounds(_map.Width, _map.Height);

        _player.Position = result.PlayerStart;
        _camera.CenterOn(_player.Position.X, _player.Position.Y);

        _visible = new bool[_map.Width, _map.Height];
        _explored = new bool[_map.Width, _map.Height];
        RecomputeFov();

        _messageBg = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
        _messageBg.SetData([new Color(0, 0, 0, 180)]);

        _panelBg = new Texture2D(Game1.Instance.GraphicsDevice, 1, 1);
        _panelBg.SetData([new Color(0, 0, 0, 220)]);

        _messageLog.Add("You descend into the dungeon...", Color.Gold);
        _messageLog.Add("Explore carefully: traps may be hidden in the dark.", Color.Gray);
        _messageLog.Add("Press Enter on < to return, > to go deeper (placeholder).", Color.Gray);
        _messageLog.Add("Press G to pick up items. Press I to open inventory.", Color.Gray);

        SeedStarterItems();
    }

    public override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();
        HandlePlayerTurn(kb);
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
            var tx = _player.Position.X + dx;
            var ty = _player.Position.Y + dy;
            if (_map.IsWalkable(tx, ty))
            {
                _player.Position = new Point(tx, ty);
                ResolveStepEffects();
            }
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
            RecomputeFov();
            EffectSystem.Tick(_player);
            RecalculatePlayerCombat();
        }
    }

    private void ResolveStepEffects()
    {
        var tile = _map.GetTile(_player.Position.X, _player.Position.Y);
        if (tile.TileDefId == "spike_trap")
        {
            var stats = _player.GetComponent<Entities.StatsComponent>();
            var dmg = 3 + ((_player.Position.X + _player.Position.Y + _seed) % 4); // 3-6 deterministic
            stats?.ApplyDamage(dmg);
            _messageLog.Add($"You step on a spike trap! Took {dmg} damage.", Color.OrangeRed);
            _map.SetTile(_player.Position.X, _player.Position.Y, "stone_floor");
        }
    }

    private void TryInteract()
    {
        var tile = _map.GetTile(_player.Position.X, _player.Position.Y);
        if (tile.TileDefId == "stairs_up")
        {
            _player.Position = _returnPosition;
            Game1.Instance.SceneManager.Pop();
            return;
        }

        if (tile.TileDefId == "stairs_down")
        {
            _messageLog.Add("The way down is blocked... for now.", Color.Gray);
        }
    }

    private void RecomputeFov()
    {
        _visible = FovCalculator.Compute(_map, _player.Position, radius: 12);
        for (var x = 0; x < _map.Width; x++)
        for (var y = 0; y < _map.Height; y++)
            if (_visible[x, y])
                _explored[x, y] = true;
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
            if (!_explored[x, y]) continue;

            var tile = _map.GetTile(x, y);
            var screenRect = _camera.GetTileScreenRect(x, y);
            var baseColor = tile.GetColor() ?? Color.White;

            var inSight = _visible[x, y];
            var bgAlpha = inSight ? 0.18f : 0.07f;
            var fg = inSight ? baseColor : (baseColor * 0.45f);

            spriteBatch.Draw(_tileTexture, screenRect, baseColor * bgAlpha);

            var glyph = tile.Glyph;
            var glyphSize = font.MeasureString(glyph);
            var pos = new Vector2(
                screenRect.X + (screenRect.Width - glyphSize.X) / 2f,
                screenRect.Y + (screenRect.Height - glyphSize.Y) / 2f
            );
            spriteBatch.DrawString(font, glyph, pos, fg);
        }

        if (_visible[_player.Position.X, _player.Position.Y] && _camera.IsTileVisible(_player.Position.X, _player.Position.Y))
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
        // Drop a couple items near stairs_up so players can test pickup/use in dungeon too.
        var positions = new[]
        {
            new Point(_stairsUp.X + 1, _stairsUp.Y),
            new Point(_stairsUp.X, _stairsUp.Y + 1)
        };
        var ids = new[] { "health_potion", "scroll_fireball" };

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
                ConsumeAction();
            }
            else if (item.Def?.Category is "weapon" or "armor" or "shield" or "accessory")
            {
                equip.Equip(item);
                _messageLog.Add($"Equipped {item.DisplayName}.", Color.LightGreen);
                RecalculatePlayerCombat();
                _inventoryOpen = false;
                ConsumeAction();
            }
            else
            {
                _messageLog.Add("That can't be equipped.", Color.Gray);
            }
        }

        if (kb.IsKeyDown(Keys.U) && _prevKeyboard.IsKeyUp(Keys.U))
        {
            if (ItemUseSystem.TryUse(_player, item, _map, enemies: null, out var msg))
            {
                _messageLog.Add(msg, Color.LightGreen);
                inv.RemoveItem(item.Id, 1);
                RecalculatePlayerCombat();
                RecomputeFov();
                _inventoryOpen = false;
                ConsumeAction();
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
            ConsumeAction();

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

    private void ConsumeAction()
    {
        _camera.CenterOn(_player.Position.X, _player.Position.Y);
        EffectSystem.Tick(_player);
        RecalculatePlayerCombat();
        RecomputeFov();
    }
}

