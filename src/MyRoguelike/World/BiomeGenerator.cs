namespace MyRoguelike.World;

public enum Biome
{
    DeepWater,
    ShallowWater,
    Sand,
    Plains,
    Forest,
    Hills,
    Mountains,
    Swamp,
    Tundra,
    Desert
}

public static class BiomeExtensions
{
    public static string ToTileId(this Biome biome) => biome switch
    {
        Biome.DeepWater => "water",
        Biome.ShallowWater => "water",
        Biome.Sand => "sand",
        Biome.Plains => "grass",
        Biome.Forest => "grass",
        Biome.Hills => "dirt",
        Biome.Mountains => "stone_wall",
        Biome.Swamp => "water",
        Biome.Tundra => "snow",
        Biome.Desert => "sand",
        _ => "grass"
    };

    public static bool HasTrees(this Biome biome) => biome == Biome.Forest;

    public static bool IsWalkable(this Biome biome) => biome != Biome.DeepWater && biome != Biome.Mountains;
}

public class BiomeGenerator
{
    private readonly int _seed;
    private readonly Random _rng;

    public BiomeGenerator(int seed)
    {
        _seed = seed;
        _rng = new Random(seed);
    }

    public Biome[,] GenerateHeightmap(int width, int height)
    {
        var heights = new float[width, height];
        var moisture = new float[width, height];
        var biomes = new Biome[width, height];

        var heightNoise = CreateNoiseGenerator(0);
        var moistureNoise = CreateNoiseGenerator(1000);

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
        {
            var nx = (double)x / width;
            var ny = (double)y / height;

            heights[x, y] = OctaveNoise(heightNoise, nx * 4, ny * 4, 4, 0.5f);
            moisture[x, y] = OctaveNoise(moistureNoise, nx * 4, ny * 4, 3, 0.5f);
        }

        for (var x = 0; x < width; x++)
        for (var y = 0; y < height; y++)
            biomes[x, y] = ClassifyBiome(heights[x, y], moisture[x, y]);

        return biomes;
    }

    private static Biome ClassifyBiome(float height, float moisture)
    {
        if (height < 0.25f) return Biome.DeepWater;
        if (height < 0.35f) return Biome.ShallowWater;
        if (height < 0.40f && moisture < 0.3f) return Biome.Sand;
        if (height < 0.40f) return Biome.Swamp;
        if (height < 0.55f)
        {
            if (moisture < 0.3f) return Biome.Desert;
            if (moisture < 0.6f) return Biome.Plains;
            return Biome.Forest;
        }
        if (height < 0.70f)
        {
            if (moisture < 0.3f) return Biome.Tundra;
            return Biome.Hills;
        }
        return Biome.Mountains;
    }

    private NoiseGenerator CreateNoiseGenerator(int offset)
    {
        var perm = new int[512];
        var source = new int[256];

        for (var i = 0; i < 256; i++)
            source[i] = i;

        for (var i = 255; i > 0; i--)
        {
            var j = _rng.Next(i + 1);
            (source[i], source[j]) = (source[j], source[i]);
        }

        for (var i = 0; i < 512; i++)
            perm[i] = source[i & 255];

        return new NoiseGenerator(perm, _seed + offset);
    }

    private static float OctaveNoise(NoiseGenerator noise, double x, double y, int octaves, float persistence)
    {
        var total = 0f;
        var frequency = 1f;
        var amplitude = 1f;
        var maxValue = 0f;

        for (var i = 0; i < octaves; i++)
        {
            total += (float)noise.Noise(x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= 2;
        }

        return (total / maxValue + 1f) / 2f;
    }

    private readonly struct NoiseGenerator
    {
        private readonly int[] _perm;
        private readonly int _seed;

        public NoiseGenerator(int[] perm, int seed)
        {
            _perm = perm;
            _seed = seed;
        }

        public double Noise(double x, double y)
        {
            x += _seed;
            y += _seed * 31;

            var xi = (int)Math.Floor(x) & 255;
            var yi = (int)Math.Floor(y) & 255;

            var xf = x - Math.Floor(x);
            var yf = y - Math.Floor(y);

            var u = Fade(xf);
            var v = Fade(yf);

            var aa = _perm[_perm[xi] + yi];
            var ab = _perm[_perm[xi] + yi + 1];
            var ba = _perm[_perm[xi + 1] + yi];
            var bb = _perm[_perm[xi + 1] + yi + 1];

            var x1 = Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1, yf), u);
            var x2 = Lerp(Grad(ab, xf, yf - 1), Grad(bb, xf - 1, yf - 1), u);

            return Lerp(x1, x2, v);
        }

        private static double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);
        private static double Lerp(double a, double b, double t) => a + t * (b - a);

        private static double Grad(int hash, double x, double y)
        {
            return (hash & 3) switch
            {
                0 => x + y,
                1 => -x + y,
                2 => x - y,
                _ => -x - y,
            };
        }
    }
}
