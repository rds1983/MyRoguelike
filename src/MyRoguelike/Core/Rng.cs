namespace MyRoguelike;

public class Rng
{
    private readonly Random _random;

    public Rng(int seed)
    {
        _random = new Random(seed);
    }

    public Rng() : this(Environment.TickCount)
    {
    }

    public int Next() => _random.Next();

    public int Next(int maxValue) => _random.Next(maxValue);

    public int Next(int minValue, int maxValue) => _random.Next(minValue, maxValue);

    public double NextDouble() => _random.NextDouble();

    public float NextFloat() => (float)_random.NextDouble();

    public bool Chance(double probability) => _random.NextDouble() < probability;
}
