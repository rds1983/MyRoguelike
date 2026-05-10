using Microsoft.Xna.Framework;
using MyRoguelike.Data;
using MyRoguelike.Systems;
using MyRoguelike.World;

namespace MyRoguelike.Tests;

public class WorldGenTests
{
    public WorldGenTests()
    {
        if (Game1.Data == null)
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Json");
            var dm = new DataManager(dataDir);
            dm.LoadAll();
            Game1.Data = dm;
        }
    }

    // ── World ─────────────────────────────────────────────────────────

    [Fact]
    public void World_Create_HasSeedAndMap()
    {
        var map = new Map(50, 40);
        var world = new World.World(42, map);

        Assert.Equal(42, world.Seed);
        Assert.Same(map, world.Map);
        Assert.NotNull(world.Regions);
    }

    // ── Region ────────────────────────────────────────────────────────

    [Fact]
    public void Region_Contains_ReturnsCorrect()
    {
        var region = new Region { X = 10, Y = 10, Width = 5, Height = 5 };

        Assert.True(region.Contains(10, 10));
        Assert.True(region.Contains(14, 14));
        Assert.False(region.Contains(9, 10));
        Assert.False(region.Contains(15, 15));
    }

    [Fact]
    public void Region_Center_CalculatesCorrectly()
    {
        var region = new Region { X = 10, Y = 10, Width = 5, Height = 5 };

        Assert.Equal(12, region.CenterX);
        Assert.Equal(12, region.CenterY);
    }

    // ── NameGenerator ─────────────────────────────────────────────────

    [Fact]
    public void NameGenerator_GenerateName_ReturnsNonEmpty()
    {
        var rng = new Random(42);

        for (var i = 0; i < 50; i++)
        {
            var name = NameGenerator.GenerateName(rng);
            Assert.False(string.IsNullOrEmpty(name));
            Assert.True(char.IsUpper(name[0]));
        }
    }

    [Fact]
    public void NameGenerator_GenerateCityName_ReturnsNonEmpty()
    {
        var rng = new Random(42);

        for (var i = 0; i < 50; i++)
        {
            var name = NameGenerator.GenerateCityName(rng);
            Assert.False(string.IsNullOrEmpty(name));
            Assert.True(char.IsUpper(name[0]));
        }
    }

    [Fact]
    public void NameGenerator_GenerateDungeonName_ReturnsNonEmpty()
    {
        var rng = new Random(42);

        for (var i = 0; i < 50; i++)
        {
            var name = NameGenerator.GenerateDungeonName(rng);
            Assert.False(string.IsNullOrEmpty(name));
            Assert.True(name.Length > 5);
        }
    }

    [Fact]
    public void NameGenerator_DeterministicSeed()
    {
        var rng1 = new Random(42);
        var rng2 = new Random(42);

        var name1 = NameGenerator.GenerateName(rng1);
        var name2 = NameGenerator.GenerateName(rng2);

        Assert.Equal(name1, name2);
    }

    // ── BiomeGenerator ────────────────────────────────────────────────

    [Fact]
    public void BiomeGenerator_GeneratesCorrectSize()
    {
        var bg = new BiomeGenerator(42);
        var biomes = bg.GenerateHeightmap(50, 40);

        Assert.Equal(50, biomes.GetLength(0));
        Assert.Equal(40, biomes.GetLength(1));
    }

    [Fact]
    public void BiomeGenerator_DeterministicSeed()
    {
        var bg1 = new BiomeGenerator(42);
        var bg2 = new BiomeGenerator(42);

        var b1 = bg1.GenerateHeightmap(10, 10);
        var b2 = bg2.GenerateHeightmap(10, 10);

        for (var x = 0; x < 10; x++)
        for (var y = 0; y < 10; y++)
            Assert.Equal(b1[x, y], b2[x, y]);
    }

    [Fact]
    public void BiomeGenerator_DifferentSeeds_DifferentOutput()
    {
        var bg1 = new BiomeGenerator(42);
        var bg2 = new BiomeGenerator(999);

        var b1 = bg1.GenerateHeightmap(10, 10);
        var b2 = bg2.GenerateHeightmap(10, 10);

        var different = false;
        for (var x = 0; x < 10 && !different; x++)
        for (var y = 0; y < 10 && !different; y++)
            if (b1[x, y] != b2[x, y]) different = true;

        Assert.True(different);
    }

    [Fact]
    public void BiomeGenerator_ContainsAllBiomeTypes()
    {
        var bg = new BiomeGenerator(42);
        var biomes = bg.GenerateHeightmap(100, 100);

        var found = new HashSet<Biome>();
        for (var x = 0; x < 100; x++)
        for (var y = 0; y < 100; y++)
            found.Add(biomes[x, y]);

        Assert.Contains(Biome.DeepWater, found);
        Assert.Contains(Biome.Plains, found);
        Assert.Contains(Biome.Mountains, found);
    }

    // ── BiomeExtensions ───────────────────────────────────────────────

    [Fact]
    public void BiomeExtensions_ToTileId_ReturnsValid()
    {
        foreach (Biome biome in Enum.GetValues<Biome>())
        {
            var tileId = biome.ToTileId();
            Assert.NotNull(tileId);
            Assert.NotEmpty(tileId);
        }
    }

    [Fact]
    public void BiomeExtensions_DeepWater_NotWalkable()
    {
        Assert.False(Biome.DeepWater.IsWalkable());
    }

    [Fact]
    public void BiomeExtensions_Plains_IsWalkable()
    {
        Assert.True(Biome.Plains.IsWalkable());
    }

    [Fact]
    public void BiomeExtensions_Forest_HasTrees()
    {
        Assert.True(Biome.Forest.HasTrees());
    }

    [Fact]
    public void BiomeExtensions_Plains_NoTrees()
    {
        Assert.False(Biome.Plains.HasTrees());
    }

    // ── WorldGenerator ────────────────────────────────────────────────

    [Fact]
    public void WorldGenerator_Generate_ReturnsWorldWithCorrectSize()
    {
        var generator = new WorldGenerator(42);
        var world = generator.Generate(50, 40);

        Assert.Equal(50, world.Map.Width);
        Assert.Equal(40, world.Map.Height);
        Assert.Equal(42, world.Seed);
    }

    [Fact]
    public void WorldGenerator_Generate_HasRegions()
    {
        var generator = new WorldGenerator(42);
        var world = generator.Generate(100, 100);

        Assert.NotEmpty(world.Regions);
    }

    [Fact]
    public void WorldGenerator_Deterministic()
    {
        var gen1 = new WorldGenerator(42);
        var gen2 = new WorldGenerator(42);

        var w1 = gen1.Generate(20, 20);
        var w2 = gen2.Generate(20, 20);

        for (var x = 0; x < 20; x++)
        for (var y = 0; y < 20; y++)
            Assert.Equal(w1.Map.GetTile(x, y).TileDefId, w2.Map.GetTile(x, y).TileDefId);
    }

    [Fact]
    public void WorldGenerator_FindPlayerSpawn_ReturnsWalkableTile()
    {
        var generator = new WorldGenerator(42);
        var world = generator.Generate(100, 100);
        var spawn = generator.FindPlayerSpawn(world);

        Assert.True(world.Map.IsWalkable(spawn.X, spawn.Y));
        Assert.InRange(spawn.X, 0, 99);
        Assert.InRange(spawn.Y, 0, 99);
    }

    [Fact]
    public void WorldGenerator_Generate_AllTilesValid()
    {
        var generator = new WorldGenerator(42);
        var world = generator.Generate(50, 50);

        for (var x = 0; x < 50; x++)
        for (var y = 0; y < 50; y++)
        {
            var tile = world.Map.GetTile(x, y);
            Assert.NotNull(tile.TileDefId);
        }
    }

    // ── SettlementGenerator ───────────────────────────────────────────

    [Fact]
    public void SettlementGenerator_Generate_ReturnsRegions()
    {
        var map = new Map(100, 100);
        var bg = new BiomeGenerator(42);
        var biomes = bg.GenerateHeightmap(100, 100);
        var gen = new SettlementGenerator(42, map, biomes);
        var regions = gen.Generate();

        Assert.NotEmpty(regions);
    }

    [Fact]
    public void SettlementGenerator_GeneratesCitiesVillagesDungeons()
    {
        var map = new Map(100, 100);
        var bg = new BiomeGenerator(42);
        var biomes = bg.GenerateHeightmap(100, 100);
        var gen = new SettlementGenerator(42, map, biomes);
        var regions = gen.Generate();

        Assert.Contains(regions, r => r.Type == RegionType.City);
        Assert.Contains(regions, r => r.Type == RegionType.Village);
        Assert.Contains(regions, r => r.Type == RegionType.Dungeon);
    }

    [Fact]
    public void SettlementGenerator_Deterministic()
    {
        var map1 = new Map(100, 100);
        var bg1 = new BiomeGenerator(42);
        var biomes1 = bg1.GenerateHeightmap(100, 100);
        var gen1 = new SettlementGenerator(42, map1, biomes1);

        var map2 = new Map(100, 100);
        var bg2 = new BiomeGenerator(42);
        var biomes2 = bg2.GenerateHeightmap(100, 100);
        var gen2 = new SettlementGenerator(42, map2, biomes2);

        var r1 = gen1.Generate();
        var r2 = gen2.Generate();

        Assert.Equal(r1.Count, r2.Count);
        for (var i = 0; i < r1.Count; i++)
        {
            Assert.Equal(r1[i].Name, r2[i].Name);
            Assert.Equal(r1[i].Type, r2[i].Type);
            Assert.Equal(r1[i].X, r2[i].X);
            Assert.Equal(r1[i].Y, r2[i].Y);
            Assert.Equal(r1[i].Width, r2[i].Width);
            Assert.Equal(r1[i].Height, r2[i].Height);
        }
    }

    [Fact]
    public void SettlementGenerator_SmallMap_ReturnsEmpty()
    {
        var map = new Map(20, 20);
        var bg = new BiomeGenerator(42);
        var biomes = bg.GenerateHeightmap(20, 20);
        var gen = new SettlementGenerator(42, map, biomes);
        var regions = gen.Generate();

        Assert.Empty(regions);
    }

    [Fact]
    public void SettlementGenerator_CitiesHaveStoneWalls()
    {
        var map = new Map(100, 100);
        var bg = new BiomeGenerator(42);
        var biomes = bg.GenerateHeightmap(100, 100);
        var gen = new SettlementGenerator(42, map, biomes);
        var regions = gen.Generate();

        var city = regions.FirstOrDefault(r => r.Type == RegionType.City);
        Assert.NotNull(city);

        var hasWalls = false;
        for (var x = city.X; x < city.X + city.Width; x++)
        for (var y = city.Y; y < city.Y + city.Height; y++)
            if (map.GetTile(x, y).TileDefId == "stone_wall")
                hasWalls = true;

        Assert.True(hasWalls, "City should contain stone_wall tiles");
    }

    [Fact]
    public void SettlementGenerator_DungeonsHaveStairsDown()
    {
        var map = new Map(100, 100);
        var bg = new BiomeGenerator(42);
        var biomes = bg.GenerateHeightmap(100, 100);
        var gen = new SettlementGenerator(42, map, biomes);
        var regions = gen.Generate();

        var dungeon = regions.FirstOrDefault(r => r.Type == RegionType.Dungeon);
        Assert.NotNull(dungeon);

        var hasStairs = false;
        for (var x = dungeon.X; x < dungeon.X + dungeon.Width; x++)
        for (var y = dungeon.Y; y < dungeon.Y + dungeon.Height; y++)
            if (map.GetTile(x, y).TileDefId == "stairs_down")
                hasStairs = true;

        Assert.True(hasStairs, "Dungeon should contain stairs_down tile");
    }

    [Fact]
    public void SettlementGenerator_RoadsExistBetweenSettlements()
    {
        var map = new Map(100, 100);
        var bg = new BiomeGenerator(42);
        var biomes = bg.GenerateHeightmap(100, 100);
        var gen = new SettlementGenerator(42, map, biomes);
        gen.Generate();

        var hasRoads = false;
        for (var x = 0; x < 100; x++)
        for (var y = 0; y < 100; y++)
            if (map.GetTile(x, y).TileDefId == "road")
                hasRoads = true;

        Assert.True(hasRoads, "Roads should exist between settlements");
    }

    [Fact]
    public void SettlementGenerator_WorldGeneratorIntegration()
    {
        var gen = new WorldGenerator(42);
        var world = gen.Generate(100, 100);

        Assert.NotEmpty(world.Regions);
        Assert.Contains(world.Regions, r => r.Type == RegionType.City);
        Assert.Contains(world.Regions, r => r.Type == RegionType.Village);
        Assert.Contains(world.Regions, r => r.Type == RegionType.Dungeon);
    }

    [Fact]
    public void WorldGenerator_FindPlayerSpawn_NotInsideBuilding()
    {
        var generator = new WorldGenerator(42);
        var world = generator.Generate(100, 100);
        var spawn = generator.FindPlayerSpawn(world);

        var tile = world.Map.GetTile(spawn.X, spawn.Y);
        Assert.Equal("grass", tile.TileDefId);
    }
}
