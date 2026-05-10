using Microsoft.Xna.Framework;
using MyRoguelike.World;
using Point = Microsoft.Xna.Framework.Point;

namespace MyRoguelike.Systems;

public static class PathfindingSystem
{
    public static List<Point> FindPath(Map map, Point start, Point end,
        Func<Point, bool>? isBlocked = null)
    {
        var startKey = (start.X, start.Y);
        var endKey = (end.X, end.Y);

        if (startKey == endKey)
            return [start];

        if (!map.IsInBounds(start.X, start.Y) || !map.IsInBounds(end.X, end.Y))
            return [];

        var frontier = new Queue<(int X, int Y)>();
        var cameFrom = new Dictionary<(int, int), (int, int)>();
        var visited = new HashSet<(int, int)>();

        frontier.Enqueue((start.X, start.Y));
        cameFrom[startKey] = startKey;
        visited.Add(startKey);

        var directions = new[] { (0, -1), (0, 1), (-1, 0), (1, 0) };

        while (frontier.Count > 0)
        {
            var current = frontier.Dequeue();
            var currentKey = (current.X, current.Y);

            if (currentKey == endKey)
                return ReconstructPath(cameFrom, currentKey);

            foreach (var (dx, dy) in directions)
            {
                var nx = current.X + dx;
                var ny = current.Y + dy;
                var neighborKey = (nx, ny);

                if (visited.Contains(neighborKey))
                    continue;

                if (!map.IsInBounds(nx, ny))
                    continue;

                if (!map.IsWalkable(nx, ny))
                    continue;

                if (isBlocked?.Invoke(new Point(nx, ny)) == true)
                    continue;

                visited.Add(neighborKey);
                cameFrom[neighborKey] = currentKey;
                frontier.Enqueue((nx, ny));
            }
        }

        return [];
    }

    private static List<Point> ReconstructPath(
        Dictionary<(int, int), (int, int)> cameFrom, (int, int) current)
    {
        var path = new List<Point>();
        var node = current;

        while (true)
        {
            path.Add(new Point(node.Item1, node.Item2));
            if (!cameFrom.TryGetValue(node, out var prev))
                break;
            if (prev == node)
                break;
            node = prev;
        }

        path.Reverse();
        return path;
    }
}
