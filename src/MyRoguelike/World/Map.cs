namespace MyRoguelike.World;

public class Map
{
    private readonly Tile[,] _tiles;

    public int Width { get; }
    public int Height { get; }

    public Map(int width, int height, string defaultTileId = "grass")
    {
        Width = width;
        Height = height;
        _tiles = new Tile[width, height];

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            _tiles[x, y] = new Tile
            {
                X = x,
                Y = y,
                TileDefId = defaultTileId
            };
        }
    }

    public Tile GetTile(int x, int y)
    {
        if (!IsInBounds(x, y))
            throw new ArgumentOutOfRangeException($"Position ({x},{y}) is out of map bounds ({Width}x{Height})");
        return _tiles[x, y];
    }

    public void SetTile(int x, int y, string tileDefId)
    {
        if (!IsInBounds(x, y)) return;
        _tiles[x, y].TileDefId = tileDefId;
    }

    public void SetTile(int x, int y, Tile tile)
    {
        if (!IsInBounds(x, y)) return;
        _tiles[x, y] = tile;
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < Width && y >= 0 && y < Height;
    }

    public bool IsWalkable(int x, int y)
    {
        return IsInBounds(x, y) && _tiles[x, y].IsWalkable;
    }

    public bool IsTransparent(int x, int y)
    {
        return IsInBounds(x, y) && _tiles[x, y].IsTransparent;
    }

    public void Fill(string tileDefId)
    {
        for (var x = 0; x < Width; x++)
        for (var y = 0; y < Height; y++)
            _tiles[x, y].TileDefId = tileDefId;
    }

    public void FillRect(int startX, int startY, int endX, int endY, string tileDefId)
    {
        for (var x = startX; x <= endX && x < Width; x++)
        for (var y = startY; y <= endY && y < Height; y++)
            if (IsInBounds(x, y))
                _tiles[x, y].TileDefId = tileDefId;
    }
}
