using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MyRoguelike.Entities;
using MyRoguelike.World;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace MyRoguelike.Scenes;

public class OverworldScene : Scene
{
    private readonly Map _map;
    private readonly Camera _camera;
    private Texture2D _tileTexture = null!;
    private Player _player = null!;

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
            Position = new Microsoft.Xna.Framework.Point(TestMapWidth / 2, TestMapHeight / 2),
            Glyph = "@",
            Color = Microsoft.Xna.Framework.Color.White,
            ClassId = "warrior"
        };

        _camera.CenterOn(_player.Position.X, _player.Position.Y);
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

    public override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();
        var moved = false;
        var dx = 0;
        var dy = 0;

        if (kb.IsKeyDown(Keys.W) || kb.IsKeyDown(Keys.Up)) dy = -1;
        else if (kb.IsKeyDown(Keys.S) || kb.IsKeyDown(Keys.Down)) dy = 1;
        else if (kb.IsKeyDown(Keys.A) || kb.IsKeyDown(Keys.Left)) dx = -1;
        else if (kb.IsKeyDown(Keys.D) || kb.IsKeyDown(Keys.Right)) dx = 1;

        if (dx != 0 || dy != 0)
            moved = TryMovePlayer(dx, dy);

        if (!moved && kb.IsKeyDown(Keys.Enter) && _prevKeyboard.IsKeyUp(Keys.Enter))
            TryInteract();

        _prevKeyboard = kb;

        if (moved)
            _camera.CenterOn(_player.Position.X, _player.Position.Y);
    }

    private bool TryMovePlayer(int dx, int dy)
    {
        var newX = _player.Position.X + dx;
        var newY = _player.Position.Y + dy;

        if (!_map.IsInBounds(newX, newY))
            return false;

        if (!_map.IsWalkable(newX, newY))
            return false;

        _player.Position = new Microsoft.Xna.Framework.Point(newX, newY);
        return true;
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

        device.Clear(Microsoft.Xna.Framework.Color.Black);
        spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend);

        var font = Game1.Font;
        for (var x = 0; x < _map.Width; x++)
        for (var y = 0; y < _map.Height; y++)
        {
            if (!_camera.IsTileVisible(x, y)) continue;

            var tile = _map.GetTile(x, y);
            var screenRect = _camera.GetTileScreenRect(x, y);
            var color = tile.GetColor() ?? Microsoft.Xna.Framework.Color.White;

            spriteBatch.Draw(_tileTexture, screenRect, color * 0.15f);

            var glyph = tile.Glyph;
            var glyphSize = font.MeasureString(glyph);
            var pos = new Vector2(
                screenRect.X + (screenRect.Width - glyphSize.X) / 2f,
                screenRect.Y + (screenRect.Height - glyphSize.Y) / 2f
            );
            spriteBatch.DrawString(font, glyph, pos, color);
        }

        // Draw player
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
    }
}
