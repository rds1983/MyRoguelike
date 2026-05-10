using Microsoft.Xna.Framework;
using MyRoguelike.World;

namespace MyRoguelike.Tests;

public class DungeonGeneratorTests
{
    public DungeonGeneratorTests()
    {
        if (Game1.Data == null)
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Json");
            var dm = new Data.DataManager(dataDir);
            dm.LoadAll();
            Game1.Data = dm;
        }
    }

    [Fact]
    public void DungeonGenerator_Generate_PlacesStairsUpAndDown()
    {
        var gen = new DungeonGenerator(12345);
        var result = gen.Generate(80, 60);

        Assert.Equal("stairs_up", result.Map.GetTile(result.StairsUp.X, result.StairsUp.Y).TileDefId);
        Assert.Equal("stairs_down", result.Map.GetTile(result.StairsDown.X, result.StairsDown.Y).TileDefId);
        Assert.NotEqual(result.StairsUp, result.StairsDown);
    }

    [Fact]
    public void DungeonGenerator_Generate_CreatesSomeFloors()
    {
        var gen = new DungeonGenerator(42);
        var result = gen.Generate(80, 60);

        var floors = 0;
        for (var x = 0; x < result.Map.Width; x++)
        for (var y = 0; y < result.Map.Height; y++)
            if (result.Map.GetTile(x, y).TileDefId is "stone_floor" or "stairs_up" or "stairs_down")
                floors++;

        Assert.True(floors > (result.Map.Width * result.Map.Height) / 12, "Dungeon should contain meaningful walkable area");
    }

    [Fact]
    public void DungeonGenerator_Generate_PlacesSomeTraps()
    {
        var gen = new DungeonGenerator(777);
        var result = gen.Generate(80, 60);

        var traps = 0;
        for (var x = 0; x < result.Map.Width; x++)
        for (var y = 0; y < result.Map.Height; y++)
            if (result.Map.GetTile(x, y).TileDefId == "spike_trap")
                traps++;

        Assert.True(traps >= 1, "Dungeon should place at least one trap");
    }
}

