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

    private bool[,] _visible = null!;
    private bool[,] _explored = null!;

    private Point _stairsUp;
    private Point _stairsDown;

    private KeyboardState _prevKeyboard;

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

        _messageLog.Add("You descend into the dungeon...", Color.Gold);
        _messageLog.Add("Explore carefully: traps may be hidden in the dark.", Color.Gray);
        _messageLog.Add("Press Enter on < to return, > to go deeper (placeholder).", Color.Gray);
    }

    public override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();
        HandlePlayerTurn(kb);
        _prevKeyboard = kb;
    }

    private void HandlePlayerTurn(KeyboardState kb)
    {
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

        if (!acted && kb.IsKeyDown(Keys.Enter) && _prevKeyboard.IsKeyUp(Keys.Enter))
        {
            TryInteract();
            acted = true;
        }

        if (acted)
        {
            _camera.CenterOn(_player.Position.X, _player.Position.Y);
            RecomputeFov();
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
        spriteBatch.End();
    }
}

