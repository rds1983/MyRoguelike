using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MyRoguelike.World;
using MyRoguelike.Scenes;

namespace MyRoguelike.Tests;

public class WorldSceneTests
{
    public WorldSceneTests()
    {
        if (Game1.Data == null)
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Json");
            var dm = new Data.DataManager(dataDir);
            dm.LoadAll();
            Game1.Data = dm;
        }
    }
    // ── Map ──────────────────────────────────────────────────────────

    [Fact]
    public void Map_Create_InitializesCorrectSize()
    {
        var map = new Map(50, 40);
        Assert.Equal(50, map.Width);
        Assert.Equal(40, map.Height);
    }

    [Fact]
    public void Map_GetTile_ReturnsTile()
    {
        var map = new Map(10, 10);
        var tile = map.GetTile(5, 5);
        Assert.NotNull(tile);
        Assert.Equal(5, tile.X);
        Assert.Equal(5, tile.Y);
    }

    [Fact]
    public void Map_GetTile_OutOfBounds_Throws()
    {
        var map = new Map(10, 10);
        Assert.Throws<ArgumentOutOfRangeException>(() => map.GetTile(20, 20));
    }

    [Fact]
    public void Map_IsInBounds_ReturnsCorrect()
    {
        var map = new Map(10, 10);
        Assert.True(map.IsInBounds(0, 0));
        Assert.True(map.IsInBounds(9, 9));
        Assert.False(map.IsInBounds(-1, 0));
        Assert.False(map.IsInBounds(10, 10));
    }

    [Fact]
    public void Map_SetTile_ChangesTileDef()
    {
        var map = new Map(10, 10);
        map.SetTile(3, 3, "water");
        Assert.Equal("water", map.GetTile(3, 3).TileDefId);
    }

    [Fact]
    public void Map_SetTile_OutOfBounds_DoesNothing()
    {
        var map = new Map(10, 10);
        map.SetTile(100, 100, "water");
        Assert.Equal("grass", map.GetTile(5, 5).TileDefId);
    }

    [Fact]
    public void Map_Fill_SetsAllTiles()
    {
        var map = new Map(5, 5);
        map.Fill("stone_wall");
        for (var x = 0; x < 5; x++)
        for (var y = 0; y < 5; y++)
            Assert.Equal("stone_wall", map.GetTile(x, y).TileDefId);
    }

    [Fact]
    public void Map_FillRect_SetsRectangularArea()
    {
        var map = new Map(20, 20);
        map.Fill("grass");
        map.FillRect(5, 5, 10, 10, "water");

        // Inside rect
        Assert.Equal("water", map.GetTile(5, 5).TileDefId);
        Assert.Equal("water", map.GetTile(10, 10).TileDefId);
        // Outside rect
        Assert.Equal("grass", map.GetTile(4, 5).TileDefId);
        Assert.Equal("grass", map.GetTile(5, 4).TileDefId);
    }

    [Fact]
    public void Map_IsWalkable_ReturnsFromTile()
    {
        var map = new Map(10, 10);
        Assert.True(map.IsWalkable(5, 5)); // grass is walkable
        map.SetTile(5, 5, "stone_wall");
        Assert.False(map.IsWalkable(5, 5)); // wall is not
    }

    [Fact]
    public void Map_IsWalkable_OutOfBounds_ReturnsFalse()
    {
        var map = new Map(10, 10);
        Assert.False(map.IsWalkable(20, 20));
    }

    // ── Camera ───────────────────────────────────────────────────────

    [Fact]
    public void Camera_CenterOn_SetsPosition()
    {
        var cam = new Camera(1280, 720);
        cam.SetMapBounds(100, 100);
        cam.CenterOn(50, 50);
        // X = 50 - 1280 / (2 * 32) = 50 - 20 = 30
        // Y = 50 - 720 / (2 * 32) = 50 - 11 = 39
        Assert.Equal(30, cam.X);
        Assert.Equal(39, cam.Y);
    }

    [Fact]
    public void Camera_Move_ChangesPosition()
    {
        var cam = new Camera(1280, 720);
        cam.SetMapBounds(100, 100);
        cam.CenterOn(50, 50);
        cam.Move(5, 3);
        Assert.Equal(35, cam.X);
        Assert.Equal(42, cam.Y);
    }

    [Fact]
    public void Camera_ClampsToBounds()
    {
        var cam = new Camera(1280, 720);
        cam.SetMapBounds(30, 20);
        cam.CenterOn(100, 100);
        // Should clamp to max
        Assert.True(cam.X >= 0);
        Assert.True(cam.Y >= 0);
    }

    [Fact]
    public void Camera_WorldToScreen_ComputesCorrectly()
    {
        var cam = new Camera(1280, 720);
        cam.SetMapBounds(100, 100);
        cam.CenterOn(10, 10);
        // CenterOn(10,10) clamps to X=0,Y=0 (can't go negative)
        var screen = cam.WorldToScreen(10, 10);
        Assert.Equal(320, screen.X);
        Assert.Equal(320, screen.Y);
    }

    [Fact]
    public void Camera_ScreenToWorld_ComputesCorrectly()
    {
        var cam = new Camera(1280, 720);
        cam.SetMapBounds(100, 100);
        cam.CenterOn(10, 10);
        var world = cam.ScreenToWorld(320, 320);
        Assert.Equal(10, world.X);
        Assert.Equal(10, world.Y);
    }

    [Fact]
    public void Camera_IsTileVisible_Nearby_ReturnsTrue()
    {
        var cam = new Camera(1280, 720);
        cam.SetMapBounds(100, 100);
        cam.CenterOn(50, 50);
        Assert.True(cam.IsTileVisible(50, 50));
        Assert.True(cam.IsTileVisible(40, 40));
    }

    [Fact]
    public void Camera_IsTileVisible_FarAway_ReturnsFalse()
    {
        var cam = new Camera(1280, 720);
        cam.SetMapBounds(100, 100);
        cam.CenterOn(50, 50);
        Assert.False(cam.IsTileVisible(0, 0));
        Assert.False(cam.IsTileVisible(99, 99));
    }

    [Fact]
    public void Camera_GetTileScreenRect_ReturnsRect()
    {
        var cam = new Camera(1280, 720);
        cam.SetMapBounds(100, 100);
        cam.CenterOn(0, 0);
        var rect = cam.GetTileScreenRect(0, 0);
        Assert.Equal(0, rect.X);
        Assert.Equal(0, rect.Y);
        Assert.Equal(32, rect.Width);
        Assert.Equal(32, rect.Height);
    }

    // ── SceneManager ─────────────────────────────────────────────────

    private class TestScene : Scene
    {
        public bool LoadCalled { get; private set; }
        public bool UpdateCalled { get; set; }
        public bool DrawCalled { get; set; }

        public override void LoadContent()
        {
            LoadCalled = true;
        }

        public override void Update(GameTime gameTime)
        {
            UpdateCalled = true;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch, GameTime gameTime)
        {
            DrawCalled = true;
        }
    }

    [Fact]
    public void SceneManager_Push_AddsScene()
    {
        var sm = new SceneManager();
        sm.Push(new TestScene());
        Assert.Equal(1, sm.Count);
    }

    [Fact]
    public void SceneManager_Push_CallsLoadContent()
    {
        var sm = new SceneManager();
        var scene = new TestScene();
        sm.Push(scene);
        Assert.True(scene.LoadCalled);
    }

    [Fact]
    public void SceneManager_Pop_RemovesScene()
    {
        var sm = new SceneManager();
        sm.Push(new TestScene());
        sm.Pop();
        Assert.Equal(0, sm.Count);
    }

    [Fact]
    public void SceneManager_Peek_ReturnsTopScene()
    {
        var sm = new SceneManager();
        var scene1 = new TestScene();
        var scene2 = new TestScene();
        sm.Push(scene1);
        sm.Push(scene2);
        Assert.Same(scene2, sm.Peek());
    }

    [Fact]
    public void SceneManager_Peek_Empty_ReturnsNull()
    {
        var sm = new SceneManager();
        Assert.Null(sm.Peek());
    }

    [Fact]
    public void SceneManager_Update_CallsTopScene()
    {
        var sm = new SceneManager();
        var scene = new TestScene();
        sm.Push(scene);
        sm.Update(new GameTime());
        Assert.True(scene.UpdateCalled);
    }

    [Fact]
    public void SceneManager_Clear_RemovesAll()
    {
        var sm = new SceneManager();
        sm.Push(new TestScene());
        sm.Push(new TestScene());
        sm.Clear();
        Assert.Equal(0, sm.Count);
    }

    [Fact]
    public void SceneManager_Pop_Empty_ReturnsNull()
    {
        var sm = new SceneManager();
        Assert.Null(sm.Pop());
    }

    // ── Tile ─────────────────────────────────────────────────────────

    [Fact]
    public void Tile_DefaultIsGrass()
    {
        var tile = new Tile();
        Assert.Equal("grass", tile.TileDefId);
    }

    [Fact]
    public void Tile_GetColor_ReturnsColorFromDef()
    {
        var tile = new Tile { TileDefId = "water" };
        var color = tile.GetColor();
        Assert.NotNull(color);
        Assert.Equal(40, color.Value.R);
        Assert.Equal(80, color.Value.G);
        Assert.Equal(200, color.Value.B);
    }
}
