using MyRoguelike.Systems;

namespace MyRoguelike.World;

public class SettlementGenerator
{
    private readonly Random _rng;
    private readonly Map _map;
    private readonly Biome[,] _biomes;
    private readonly int _width;
    private readonly int _height;
    private readonly bool[,] _occupied;

    public SettlementGenerator(int seed, Map map, Biome[,] biomes)
    {
        _rng = new Random(seed + 10000);
        _map = map;
        _biomes = biomes;
        _width = map.Width;
        _height = map.Height;
        _occupied = new bool[_width, _height];
    }

    public List<Region> Generate()
    {
        var regions = new List<Region>();

        if (_width < 30 || _height < 30)
            return regions;

        for (var i = 0; i < 3; i++)
        {
            var region = TryPlaceSettlement(RegionType.City, 4, 8, 6);
            if (region != null) regions.Add(region);
        }

        for (var i = 0; i < 4; i++)
        {
            var region = TryPlaceSettlement(RegionType.Village, 2, 4, 4);
            if (region != null) regions.Add(region);
        }

        for (var i = 0; i < 3; i++)
        {
            var region = TryPlaceDungeon();
            if (region != null) regions.Add(region);
        }

        var roadTargets = regions.Where(r => r.Type != RegionType.Dungeon).ToList();
        ConnectWithRoads(roadTargets);

        return regions;
    }

    private Region? TryPlaceSettlement(RegionType type, int minBldgs, int maxBldgs, int spread)
    {
        for (var attempt = 0; attempt < 30; attempt++)
        {
            var cx = _rng.Next(10, _width - 10);
            var cy = _rng.Next(10, _height - 10);

            if (!_map.IsWalkable(cx, cy)) continue;

            var biome = _biomes[cx, cy];
            if (biome != Biome.Plains && biome != Biome.Forest && biome != Biome.Desert &&
                biome != Biome.Tundra && biome != Biome.Hills && biome != Biome.Sand)
                continue;

            var count = _rng.Next(minBldgs, maxBldgs + 1);
            var buildings = new List<(int x, int y, int w, int h)>();

            for (var b = 0; b < count; b++)
            {
                for (var ba = 0; ba < 15; ba++)
                {
                    var ox = _rng.Next(-spread, spread + 1);
                    var oy = _rng.Next(-spread, spread + 1);
                    var bx = cx + ox;
                    var by = cy + oy;
                    var bw = _rng.Next(3, type == RegionType.Village ? 5 : 6);
                    var bh = _rng.Next(3, type == RegionType.Village ? 5 : 6);

                    if (CanPlaceBuilding(bx, by, bw, bh))
                    {
                        PlaceBuilding(bx, by, bw, bh);
                        buildings.Add((bx, by, bw, bh));
                        break;
                    }
                }
            }

            if (buildings.Count < Math.Max(1, minBldgs / 2))
            {
                foreach (var (bx, by, bw, bh) in buildings)
                    ClearOccupied(bx, by, bw, bh);
                continue;
            }

            var minX = buildings.Min(b => b.x);
            var minY = buildings.Min(b => b.y);
            var maxX = buildings.Max(b => b.x + b.w);
            var maxY = buildings.Max(b => b.y + b.h);

            var name = type == RegionType.City
                ? NameGenerator.GenerateCityName(_rng)
                : NameGenerator.GenerateName(_rng);

            return new Region
            {
                Name = name,
                Type = type,
                X = minX,
                Y = minY,
                Width = maxX - minX,
                Height = maxY - minY
            };
        }

        return null;
    }

    private bool CanPlaceBuilding(int x, int y, int w, int h)
    {
        for (var bx = x - 1; bx < x + w + 1; bx++)
        for (var by = y - 1; by < y + h + 1; by++)
        {
            if (!_map.IsInBounds(bx, by)) return false;
            if (_occupied[bx, by]) return false;
            if (!_map.IsWalkable(bx, by)) return false;
        }
        return true;
    }

    private void PlaceBuilding(int x, int y, int w, int h)
    {
        for (var bx = x; bx < x + w; bx++)
        for (var by = y; by < y + h; by++)
        {
            _occupied[bx, by] = true;
            if (bx == x || bx == x + w - 1 || by == y || by == y + h - 1)
                _map.SetTile(bx, by, "stone_wall");
            else
                _map.SetTile(bx, by, "stone_floor");
        }

        var doorSide = _rng.Next(4);
        switch (doorSide)
        {
            case 0:
                _map.SetTile(x + 1 + _rng.Next(Math.Max(1, w - 3)), y, "door");
                break;
            case 1:
                _map.SetTile(x + 1 + _rng.Next(Math.Max(1, w - 3)), y + h - 1, "door");
                break;
            case 2:
                _map.SetTile(x, y + 1 + _rng.Next(Math.Max(1, h - 3)), "door");
                break;
            case 3:
                _map.SetTile(x + w - 1, y + 1 + _rng.Next(Math.Max(1, h - 3)), "door");
                break;
        }
    }

    private void ClearOccupied(int x, int y, int w, int h)
    {
        for (var bx = x; bx < x + w; bx++)
        for (var by = y; by < y + h; by++)
            _occupied[bx, by] = false;
    }

    private Region? TryPlaceDungeon()
    {
        for (var attempt = 0; attempt < 30; attempt++)
        {
            var dx = _rng.Next(10, _width - 10);
            var dy = _rng.Next(10, _height - 10);

            if (!_map.IsWalkable(dx, dy)) continue;
            if (_occupied[dx, dy]) continue;

            var dw = _rng.Next(4, 7);
            var dh = _rng.Next(4, 7);

            if (!CanPlaceBuilding(dx, dy, dw, dh)) continue;

            PlaceBuilding(dx, dy, dw, dh);

            _map.SetTile(dx + dw / 2, dy + dh / 2, "stairs_down");

            return new Region
            {
                Name = NameGenerator.GenerateDungeonName(_rng),
                Type = RegionType.Dungeon,
                X = dx,
                Y = dy,
                Width = dw,
                Height = dh
            };
        }
        return null;
    }

    private void ConnectWithRoads(List<Region> settlements)
    {
        if (settlements.Count < 2) return;

        var connected = new HashSet<(int, int)>();
        for (var i = 0; i < settlements.Count; i++)
        {
            var nearest = -1;
            var nearestDist = int.MaxValue;
            for (var j = 0; j < settlements.Count; j++)
            {
                if (i == j) continue;
                var dx = settlements[i].CenterX - settlements[j].CenterX;
                var dy = settlements[i].CenterY - settlements[j].CenterY;
                var dist = dx * dx + dy * dy;
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = j;
                }
            }
            if (nearest >= 0)
            {
                var key = i < nearest ? (i, nearest) : (nearest, i);
                if (!connected.Contains(key))
                {
                    connected.Add(key);
                    LayRoad(settlements[i].CenterX, settlements[i].CenterY,
                            settlements[nearest].CenterX, settlements[nearest].CenterY);
                }
            }
        }

        for (var i = 0; i < settlements.Count; i++)
        for (var j = i + 1; j < settlements.Count; j++)
        {
            if (_rng.NextDouble() < 0.25 && !connected.Contains((i, j)))
            {
                LayRoad(settlements[i].CenterX, settlements[i].CenterY,
                        settlements[j].CenterX, settlements[j].CenterY);
            }
        }
    }

    private void LayRoad(int x1, int y1, int x2, int y2)
    {
        var x = x1;
        var y = y1;

        while (x != x2)
        {
            if (_map.IsInBounds(x, y) && _map.IsWalkable(x, y) && !_occupied[x, y])
                _map.SetTile(x, y, "road");
            x += Math.Sign(x2 - x1);
        }
        while (y != y2)
        {
            if (_map.IsInBounds(x, y) && _map.IsWalkable(x, y) && !_occupied[x, y])
                _map.SetTile(x, y, "road");
            y += Math.Sign(y2 - y1);
        }
    }
}
