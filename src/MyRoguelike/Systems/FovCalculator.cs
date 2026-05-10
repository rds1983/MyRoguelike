using MyRoguelike.World;
using Point = Microsoft.Xna.Framework.Point;

namespace MyRoguelike.Systems;

public static class FovCalculator
{
    public static bool[,] Compute(Map map, Point origin, int radius)
    {
        var visible = new bool[map.Width, map.Height];
        if (!map.IsInBounds(origin.X, origin.Y)) return visible;

        visible[origin.X, origin.Y] = true;
        if (radius <= 0) return visible;

        // Symmetric shadowcasting (8 octants)
        for (var octant = 0; octant < 8; octant++)
            CastLight(map, visible, origin.X, origin.Y, radius, 1, 1.0, 0.0, octant);

        return visible;
    }

    private static void CastLight(Map map, bool[,] visible, int cx, int cy, int radius,
        int row, double startSlope, double endSlope, int octant)
    {
        if (startSlope < endSlope) return;

        var radiusSquared = radius * radius;
        for (var distance = row; distance <= radius; distance++)
        {
            var dx = -distance;
            var dy = -distance;
            var blocked = false;
            var newStart = startSlope;

            while (dx <= 0)
            {
                var lSlope = (dx - 0.5) / (dy + 0.5);
                var rSlope = (dx + 0.5) / (dy - 0.5);

                if (rSlope > startSlope)
                {
                    dx++;
                    continue;
                }

                if (lSlope < endSlope)
                    break;

                var (mx, my) = TransformOctant(cx, cy, dx, dy, octant);

                if (map.IsInBounds(mx, my))
                {
                    var dist2 = dx * dx + dy * dy;
                    if (dist2 <= radiusSquared)
                        visible[mx, my] = true;

                    var opaque = !map.IsTransparent(mx, my);

                    if (blocked)
                    {
                        if (opaque)
                        {
                            newStart = rSlope;
                        }
                        else
                        {
                            blocked = false;
                            startSlope = newStart;
                        }
                    }
                    else if (opaque && distance < radius)
                    {
                        blocked = true;
                        CastLight(map, visible, cx, cy, radius, distance + 1, startSlope, lSlope, octant);
                        newStart = rSlope;
                    }
                }

                dx++;
            }

            if (blocked) break;
        }
    }

    private static (int x, int y) TransformOctant(int cx, int cy, int dx, int dy, int octant)
    {
        return octant switch
        {
            0 => (cx + dx, cy + dy),
            1 => (cx + dy, cy + dx),
            2 => (cx + dy, cy - dx),
            3 => (cx + dx, cy - dy),
            4 => (cx - dx, cy - dy),
            5 => (cx - dy, cy - dx),
            6 => (cx - dy, cy + dx),
            7 => (cx - dx, cy + dy),
            _ => (cx, cy)
        };
    }
}

