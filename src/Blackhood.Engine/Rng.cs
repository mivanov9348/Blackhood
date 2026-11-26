namespace Blackhood.Engine;

/// <summary>
/// Small deterministic PRNG (xorshift64*) whose whole state is one number, so it can be
/// saved inside the match JSON and a match continues identically after reloading.
/// </summary>
public sealed class Rng
{
    public ulong State { get; set; }

    public Rng() : this(0x9E3779B97F4A7C15) { }

    public Rng(int seed)
    {
        State = (ulong)(uint)seed * 0x9E3779B97F4A7C15UL ^ 0xD1B54A32D192ED03UL;
        if (State == 0) State = 1;
    }

    private Rng(ulong state) => State = state;

    public ulong NextULong()
    {
        var x = State;
        x ^= x >> 12;
        x ^= x << 25;
        x ^= x >> 27;
        State = x;
        return x * 0x2545F4914F6CDD1DUL;
    }

    /// <summary>0 ≤ result &lt; max</summary>
    public int Next(int max) => max <= 1 ? 0 : (int)(NextULong() % (ulong)max);

    public double NextDouble() => (NextULong() >> 11) * (1.0 / (1UL << 53));

    public bool Chance(double p) => NextDouble() < p;

    public T Pick<T>(IReadOnlyList<T> items) => items[Next(items.Count)];

    public void Shuffle<T>(IList<T> list)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>Picks an index with probability proportional to its weight (weights ≤ 0 are never picked).</summary>
    public int Weighted(IReadOnlyList<double> weights)
    {
        var total = weights.Where(w => w > 0).Sum();
        if (total <= 0) return -1;
        var roll = NextDouble() * total;
        for (var i = 0; i < weights.Count; i++)
        {
            if (weights[i] <= 0) continue;
            roll -= weights[i];
            if (roll < 0) return i;
        }
        return weights.Count - 1;
    }
}
