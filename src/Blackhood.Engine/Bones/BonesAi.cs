namespace Blackhood.Engine.Bones;

/// <summary>
/// NPCs in Bones reason about probabilities with their own dice, then their character
/// bends the decision: the bold bid high and call late, the cautious call early, the
/// cunning bluff on faces they do not hold, the calculating are simply more accurate.
/// </summary>
public static class BonesAi
{
    /// <summary>The probability that at least <paramref name="need"/> of <paramref name="n"/> unknown dice match.</summary>
    public static double AtLeast(int n, int need, double p)
    {
        if (need <= 0) return 1;
        if (need > n) return 0;
        double total = 0;
        for (var k = need; k <= n; k++) total += Binomial(n, k) * Math.Pow(p, k) * Math.Pow(1 - p, n - k);
        return Math.Min(1, total);
    }

    private static double Binomial(int n, int k)
    {
        double r = 1;
        for (var i = 1; i <= k; i++) r = r * (n - k + i) / i;
        return r;
    }

    private static int Own(BonesPlayer p, int face) =>
        p.Dice.Count(d => d == face || (face != BonesRules.WildFace && d == BonesRules.WildFace));

    private static double Chance(int face) => face == BonesRules.WildFace ? 1.0 / 6 : 2.0 / 6;

    /// <summary>How likely this player believes the bid to be true.</summary>
    public static double Belief(BonesMatch m, BonesPlayer p, int quantity, int face)
    {
        var unknown = m.TotalDice - p.Dice.Count;
        var exact = AtLeast(unknown, quantity - Own(p, face), Chance(face));
        // A poor calculator misjudges: blend the true estimate with a coin-flip guess.
        var accuracy = 0.55 + p.Traits.Calculation * 0.09; // 0.64 … 1.0
        var noise = (m.Rng.NextDouble() - 0.5) * (1 - accuracy) * 0.8;
        return Math.Clamp(exact * accuracy + 0.5 * (1 - accuracy) + noise, 0, 1);
    }

    /// <summary>Returns the bid to make, or null to call "Liar!".</summary>
    public static Bid? Decide(BonesMatch m, BonesPlayer p)
    {
        var t = p.Traits;
        var current = m.CurrentBid;

        // Call the current bid?
        if (current is not null)
        {
            var belief = Belief(m, p, current.Quantity, current.Face);
            var callBelow = 0.42 - (t.Boldness - 3) * 0.05 + (t.FearOfLoss - 3) * 0.03;
            if (belief < callBelow) return null;
        }

        // Candidate raises: the smallest legal step for every face, and one step more.
        var candidates = new List<(int Q, int F)>();
        for (var f = 1; f <= 6; f++)
        {
            int q;
            if (current is null) q = 1;
            else q = f > current.Face ? current.Quantity : current.Quantity + 1;
            if (q <= m.TotalDice) candidates.Add((q, f));
            if (q + 1 <= m.TotalDice) candidates.Add((q + 1, f));
        }

        if (current is null)
        {
            // Opening: a safe-ish number of my best face, bolder characters open higher.
            var best = Enumerable.Range(1, 6).OrderByDescending(f => Own(p, f) + (f == 1 ? -1 : 0)).ThenByDescending(f => f).First();
            var expected = Own(p, best) + (m.TotalDice - p.Dice.Count) * Chance(best);
            var q = Math.Max(1, (int)Math.Floor(expected * (0.55 + t.Boldness * 0.07)));
            if (m.Rng.Chance(t.Cunning * 0.05)) best = m.Rng.Next(5) + 2; // a cunning opener sometimes bluffs a face
            return new Bid(Math.Min(q, m.TotalDice), best, p.Seat);
        }

        // No legal raise left (every die, sixes) — the only move is to call.
        if (candidates.Count == 0) return null;

        // Score each raise by how believable it is to me; the cunning sometimes pick a bluff.
        var scored = candidates
            .Select(c => (c.Q, c.F, B: Belief(m, p, c.Q, c.F)))
            .OrderByDescending(c => c.B)
            .ToList();
        var top = scored[0];
        var floor = 0.45 - (t.Boldness - 3) * 0.06;
        if (top.B < floor)
        {
            // Nothing good to say. The bold bluff anyway, the rest call.
            if (m.Rng.Chance(0.15 + (t.Boldness - 3) * 0.12 + (t.Cunning - 3) * 0.05)) return new Bid(top.Q, top.F, p.Seat);
            return null;
        }
        if (scored.Count > 2 && m.Rng.Chance(t.Cunning * 0.04))
        {
            var bluff = scored[m.Rng.Next(Math.Min(4, scored.Count))];
            return new Bid(bluff.Q, bluff.F, p.Seat);
        }
        return new Bid(top.Q, top.F, p.Seat);
    }

    /// <summary>A gesture when an NPC bids something it does not believe — or, for the cunning, a fake one.</summary>
    public static void MaybeShowTell(BonesMatch m, BonesPlayer p, Bid bid)
    {
        if (p.IsHuman || p.Guest is null) return;
        var lying = p.Guest.Tells.FirstOrDefault(x => x.State is TellState.Lying or TellState.PlanningBetrayal);
        var confident = p.Guest.Tells.FirstOrDefault(x => x.State == TellState.Confident);
        var unknown = m.TotalDice - p.Dice.Count;
        var exact = AtLeast(unknown, bid.Quantity - Own(p, bid.Face), Chance(bid.Face));

        Tell? show = null;
        if (exact < 0.4 && lying is not null && m.Rng.Chance((6 - p.Traits.Composure) * 0.08)) show = lying;
        else if (exact >= 0.75 && confident is not null && m.Rng.Chance(0.25)) show = confident;
        else if (exact >= 0.6 && lying is { Fakeable: true } && p.Traits.Cunning >= 4 && m.Rng.Chance(0.1)) show = lying;
        if (show is null) return;

        var d = show.Description.Trim().TrimEnd('.');
        BonesGame.Event(m, "tell", $"{p.Name} {char.ToLowerInvariant(d[0])}{d[1..]}.", p.Seat);
    }
}
