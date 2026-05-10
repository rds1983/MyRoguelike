using Point = Microsoft.Xna.Framework.Point;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MyRoguelike.World;

public sealed class DungeonGenerator
{
    public sealed record Result(Map Map, Point PlayerStart, Point StairsUp, Point StairsDown);

    private readonly Random _rng;

    public DungeonGenerator(int seed)
    {
        _rng = new Random(seed);
    }

    public Result Generate(int width, int height)
    {
        if (width < 30 || height < 30)
            throw new ArgumentOutOfRangeException(nameof(width), "Dungeon must be at least 30x30");

        var map = new Map(width, height, "stone_wall");

        var leaves = new List<RectNode>();
        var root = new RectNode(new Rectangle(1, 1, width - 2, height - 2));
        SplitBsp(root, leaves, depth: 0, maxDepth: 5);

        var rooms = new List<Rectangle>();
        foreach (var leaf in leaves)
        {
            var room = CarveRoom(map, leaf.Bounds);
            if (room.Width > 0)
                rooms.Add(room);
        }

        if (rooms.Count == 0)
        {
            // Fallback: single room
            rooms.Add(CarveRoom(map, root.Bounds));
        }

        // Connect rooms in order by corridors
        for (var i = 1; i < rooms.Count; i++)
        {
            var a = CenterOf(rooms[i - 1]);
            var b = CenterOf(rooms[i]);
            CarveCorridor(map, a, b);
        }

        var up = CenterOf(rooms.First());
        var down = CenterOf(rooms.Last());
        map.SetTile(up.X, up.Y, "stairs_up");
        map.SetTile(down.X, down.Y, "stairs_down");

        PlaceTraps(map, rooms, up, down);

        return new Result(map, up, up, down);
    }

    private void PlaceTraps(Map map, List<Rectangle> rooms, Point stairsUp, Point stairsDown)
    {
        var floorTiles = new List<Point>();
        foreach (var room in rooms)
        {
            for (var x = room.X + 1; x < room.X + room.Width - 1; x++)
            for (var y = room.Y + 1; y < room.Y + room.Height - 1; y++)
            {
                var id = map.GetTile(x, y).TileDefId;
                if (id != "stone_floor") continue;
                if (x == stairsUp.X && y == stairsUp.Y) continue;
                if (x == stairsDown.X && y == stairsDown.Y) continue;
                floorTiles.Add(new Point(x, y));
            }
        }

        if (floorTiles.Count == 0) return;

        var traps = Math.Clamp((map.Width * map.Height) / 600, 4, 12);
        for (var i = 0; i < traps && floorTiles.Count > 0; i++)
        {
            var idx = _rng.Next(floorTiles.Count);
            var p = floorTiles[idx];
            floorTiles.RemoveAt(idx);
            map.SetTile(p.X, p.Y, "spike_trap");
        }
    }

    private void SplitBsp(RectNode root, List<RectNode> leaves, int depth, int maxDepth)
    {
        if (depth >= maxDepth || (root.Bounds.Width < 18 && root.Bounds.Height < 18))
        {
            leaves.Add(root);
            return;
        }

        var canSplitH = root.Bounds.Height >= 18;
        var canSplitV = root.Bounds.Width >= 18;

        if (!canSplitH && !canSplitV)
        {
            leaves.Add(root);
            return;
        }

        var splitHorizontally = !canSplitV || (canSplitH && _rng.NextDouble() < 0.5);

        if (splitHorizontally)
        {
            var min = root.Bounds.Y + 8;
            var max = root.Bounds.Y + root.Bounds.Height - 8;
            if (max <= min)
            {
                leaves.Add(root);
                return;
            }

            var splitY = _rng.Next(min, max);
            var top = new Rectangle(root.Bounds.X, root.Bounds.Y, root.Bounds.Width, splitY - root.Bounds.Y);
            var bot = new Rectangle(root.Bounds.X, splitY, root.Bounds.Width, root.Bounds.Y + root.Bounds.Height - splitY);
            root.Left = new RectNode(top);
            root.Right = new RectNode(bot);
        }
        else
        {
            var min = root.Bounds.X + 8;
            var max = root.Bounds.X + root.Bounds.Width - 8;
            if (max <= min)
            {
                leaves.Add(root);
                return;
            }

            var splitX = _rng.Next(min, max);
            var left = new Rectangle(root.Bounds.X, root.Bounds.Y, splitX - root.Bounds.X, root.Bounds.Height);
            var right = new Rectangle(splitX, root.Bounds.Y, root.Bounds.X + root.Bounds.Width - splitX, root.Bounds.Height);
            root.Left = new RectNode(left);
            root.Right = new RectNode(right);
        }

        SplitBsp(root.Left!, leaves, depth + 1, maxDepth);
        SplitBsp(root.Right!, leaves, depth + 1, maxDepth);
    }

    private Rectangle CarveRoom(Map map, Rectangle leaf)
    {
        // room size with padding to keep walls
        var minW = 6;
        var minH = 6;
        if (leaf.Width < minW + 2 || leaf.Height < minH + 2) return Rectangle.Empty;

        var w = _rng.Next(minW, Math.Min(leaf.Width - 1, 12));
        var h = _rng.Next(minH, Math.Min(leaf.Height - 1, 12));
        var x = _rng.Next(leaf.X, leaf.X + leaf.Width - w);
        var y = _rng.Next(leaf.Y, leaf.Y + leaf.Height - h);

        for (var rx = x; rx < x + w; rx++)
        for (var ry = y; ry < y + h; ry++)
            map.SetTile(rx, ry, "stone_floor");

        // add 1-tile thick walls around room if not already wall
        for (var rx = x - 1; rx <= x + w; rx++)
        for (var ry = y - 1; ry <= y + h; ry++)
        {
            if (!map.IsInBounds(rx, ry)) continue;
            if (map.GetTile(rx, ry).TileDefId == "stone_floor") continue;
            map.SetTile(rx, ry, "stone_wall");
        }

        return new Rectangle(x, y, w, h);
    }

    private static void CarveCorridor(Map map, Point a, Point b)
    {
        // L-shaped corridor with 50/50 orientation
        var horizFirst = (a.X + a.Y + b.X + b.Y) % 2 == 0;
        if (horizFirst)
        {
            CarveLine(map, a.X, a.Y, b.X, a.Y);
            CarveLine(map, b.X, a.Y, b.X, b.Y);
        }
        else
        {
            CarveLine(map, a.X, a.Y, a.X, b.Y);
            CarveLine(map, a.X, b.Y, b.X, b.Y);
        }
    }

    private static void CarveLine(Map map, int x1, int y1, int x2, int y2)
    {
        var x = x1;
        var y = y1;
        map.SetTile(x, y, "stone_floor");

        while (x != x2 || y != y2)
        {
            if (x != x2) x += Math.Sign(x2 - x);
            else if (y != y2) y += Math.Sign(y2 - y);
            map.SetTile(x, y, "stone_floor");
        }
    }

    private static Point CenterOf(Rectangle r) => new(r.X + r.Width / 2, r.Y + r.Height / 2);

    private sealed class RectNode
    {
        public Rectangle Bounds { get; }
        public RectNode? Left { get; set; }
        public RectNode? Right { get; set; }

        public RectNode(Rectangle bounds)
        {
            Bounds = bounds;
        }
    }
}

