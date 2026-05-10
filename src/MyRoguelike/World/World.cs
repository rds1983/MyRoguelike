namespace MyRoguelike.World;

public class World
{
    public int Seed { get; }
    public Map Map { get; }
    public List<Region> Regions { get; } = [];

    public World(int seed, Map map)
    {
        Seed = seed;
        Map = map;
    }
}
