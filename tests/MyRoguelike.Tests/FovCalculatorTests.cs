using Microsoft.Xna.Framework;
using MyRoguelike.Systems;
using MyRoguelike.World;

namespace MyRoguelike.Tests;

public class FovCalculatorTests
{
    public FovCalculatorTests()
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
    public void FovCalculator_Compute_OriginAlwaysVisible()
    {
        var map = new Map(20, 20, "stone_floor");
        var origin = new Point(10, 10);
        var vis = FovCalculator.Compute(map, origin, radius: 8);

        Assert.True(vis[origin.X, origin.Y]);
    }

    [Fact]
    public void FovCalculator_Compute_WallsBlockBeyond()
    {
        var map = new Map(20, 20, "stone_floor");
        var origin = new Point(10, 10);

        // Create a vertical wall at x=12
        for (var y = 0; y < 20; y++)
            map.SetTile(12, y, "stone_wall");

        var vis = FovCalculator.Compute(map, origin, radius: 8);

        // Point behind the wall (same row) should not be visible
        Assert.False(vis[14, 10]);
        // Point before the wall should be visible
        Assert.True(vis[11, 10]);
    }
}

