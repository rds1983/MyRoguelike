using MyRoguelike.Systems;
using Point = Microsoft.Xna.Framework.Point;

namespace MyRoguelike.World;

public class WorldGenerator
{
    private readonly int _seed;
    private readonly Random _rng;

    public WorldGenerator(int seed)
    {
        _seed = seed;
        _rng = new Random(seed);
    }

    public WorldGenerator() : this(Environment.TickCount)
    {
    }

    public World Generate(int width, int height)
    {
        var biomeGen = new BiomeGenerator(_seed);
        var biomes = biomeGen.GenerateHeightmap(width, height);

        var map = new Map(width, height);

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            var biome = biomes[x, y];
            map.SetTile(x, y, biome.ToTileId());

            if (biome.HasTrees() && _rng.NextDouble() < 0.15)
                map.SetTile(x, y, "tree");
        }

        var world = new World(_seed, map);

        var settlementGen = new SettlementGenerator(_seed, map, biomes);
        world.Regions.AddRange(settlementGen.Generate());

        return world;
    }

    public Point FindPlayerSpawn(World world)
    {
        for (var attempt = 0; attempt < 100; attempt++)
        {
            var x = _rng.Next(5, world.Map.Width - 5);
            var y = _rng.Next(5, world.Map.Height - 5);

            if (world.Map.IsWalkable(x, y) && world.Map.GetTile(x, y).TileDefId == "grass")
                return new Microsoft.Xna.Framework.Point(x, y);
        }

        for (var x = 0; x < world.Map.Width; x++)
        for (var y = 0; y < world.Map.Height; y++)
            if (world.Map.IsWalkable(x, y))
                return new Microsoft.Xna.Framework.Point(x, y);

        return new Microsoft.Xna.Framework.Point(world.Map.Width / 2, world.Map.Height / 2);
    }

}
