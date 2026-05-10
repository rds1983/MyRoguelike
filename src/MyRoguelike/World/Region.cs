namespace MyRoguelike.World;

public enum RegionType
{
    Wild,
    City,
    Village,
    Dungeon
}

public class Region
{
    public string Name { get; set; } = string.Empty;
    public RegionType Type { get; set; } = RegionType.Wild;
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }

    public int CenterX => X + Width / 2;
    public int CenterY => Y + Height / 2;

    public bool Contains(int px, int py) =>
        px >= X && px < X + Width && py >= Y && py < Y + Height;
}
