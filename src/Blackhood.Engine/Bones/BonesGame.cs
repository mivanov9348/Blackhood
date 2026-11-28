namespace Blackhood.Engine.Bones;

/// <summary>
/// "Bones" — a bluffing dice game by the fire. Everyone hides five dice under a cup.
/// Players take turns bidding how many dice of one face lie on the whole table
/// (ones — the Witch's eye — count as every face). Raise the bid or call "Liar!":
/// the cups are lifted and whoever was wrong loses a die. The last player with dice
/// takes the pot.
/// </summary>
public static class BonesRules
{
    public const int Players = 4;
    public const int DicePerPlayer = 4;
    public const int BuyIn = 5;
    public const int WildFace = 1;
}

public enum BonesPhase
{
    /// <summary>Someone has to bid or call.</summary>
    Bidding,
    /// <summary>The cups are lifted; waiting for the human to continue.</summary>
    Reveal,
    Finished,
}

public sealed record Bid(int Quantity, int Face, int Seat);

public sealed record BonesEvent(int Round, string Kind, string Text, int? Seat = null);

public sealed record RevealReport(int Round, Bid Bid, int CallerSeat, int Count, bool BidWasTrue, int LoserSeat, Dictionary<int, List<int>> Dice);

public sealed class BonesPlayer
{
    public int Seat { get; set; }
    public string Name { get; set; } = "";
    public bool IsHuman { get; set; }
    public TableGuest? Guest { get; set; }
    public Traits Traits { get; set; } = Traits.Average;
    public List<int> Dice { get; set; } = [];
    public bool Out => Dice.Count == 0;
}

public sealed class BonesMatch
{
    public Guid Id { get; set; }
    public int Seed { get; set; }
    public Rng Rng { get; set; } = new();
    public int Round { get; set; } = 1;
    public BonesPhase Phase { get; set; } = BonesPhase.Bidding;
    public List<BonesPlayer> Players { get; set; } = [];
    public int TurnSeat { get; set; }
    public Bid? CurrentBid { get; set; }
    public List<Bid> RoundBids { get; set; } = [];
    public RevealReport? LastReveal { get; set; }
    public List<BonesEvent> Events { get; set; } = [];
    public int Pot { get; set; }
    public int? WinnerSeat { get; set; }

    public int TotalDice => Players.Sum(p => p.Dice.Count);
    public IEnumerable<BonesPlayer> InPlay => Players.Where(p => !p.Out);
}

public sealed class BonesRuleException(string message) : Exception(message);

public static class BonesGame
{
    public static BonesMatch Create(string? humanName, int seed, IReadOnlyList<TableGuest> pool)
    {
        var npcCount = humanName is null ? BonesRules.Players : BonesRules.Players - 1;
        if (pool.Count < npcCount) throw new InvalidOperationException($"Need at least {npcCount} guests.");

        var rng = new Rng(seed);
        var m = new BonesMatch { Id = Guid.NewGuid(), Seed = seed, Rng = rng, Pot = BonesRules.BuyIn * BonesRules.Players };
        if (humanName is not null) m.Players.Add(new BonesPlayer { Seat = 0, Name = humanName, IsHuman = true });
        var guests = pool.ToList();
        rng.Shuffle(guests);
        foreach (var g in guests.Take(npcCount))
            m.Players.Add(new BonesPlayer { Seat = m.Players.Count, Name = g.Name, Guest = g, Traits = g.Traits });
        foreach (var p in m.Players) p.Dice = Enumerable.Repeat(0, BonesRules.DicePerPlayer).ToList();

        Event(m, "start", $"Four players, four dice each. The pot holds {m.Pot} gold. Ones are the Witch's eye — they count as any face.");
        m.TurnSeat = rng.Next(m.Players.Count);
        StartRound(m);
        Run(m);
        return m;
    }

    // ------------------------------------------------------------------ human moves

    public static void Bid(BonesMatch m, int seat, int quantity, int face)
    {
        RequireTurn(m, seat);
        PlaceBid(m, seat, quantity, face);
        Run(m);
    }

    public static void CallLiar(BonesMatch m, int seat)
    {
        RequireTurn(m, seat);
        Call(m, seat);
        Run(m);
    }

    /// <summary>After a reveal: start the next round (or let the NPCs finish if the human is out).</summary>
    public static void Continue(BonesMatch m, int seat)
    {
        if (m.Phase != BonesPhase.Reveal) throw new BonesRuleException("Nothing to continue.");
        NextRound(m);
        Run(m);
    }

    // ------------------------------------------------------------------ the loop

    public static void Run(BonesMatch m)
    {
        for (var guard = 0; guard < 5000; guard++)
        {
            if (m.Phase == BonesPhase.Finished) return;
            var human = m.Players.FirstOrDefault(p => p.IsHuman);

            if (m.Phase == BonesPhase.Reveal)
            {
                // A human still in the game wants to see the cups; a human who is out (or none) does not wait.
                if (human is not null && !human.Out) return;
                NextRound(m);
                continue;
            }

            var p = m.Players[m.TurnSeat];
            if (p.IsHuman) return;
            var decision = BonesAi.Decide(m, p);
            if (decision is null) Call(m, p.Seat);
            else PlaceBid(m, p.Seat, decision.Quantity, decision.Face);
        }
        throw new InvalidOperationException("Bones loop did not settle.");
    }

    private static void StartRound(BonesMatch m)
    {
        foreach (var p in m.InPlay)
            for (var i = 0; i < p.Dice.Count; i++) p.Dice[i] = m.Rng.Next(6) + 1;
        m.CurrentBid = null;
        m.RoundBids.Clear();
        m.Phase = BonesPhase.Bidding;
        if (m.Players[m.TurnSeat].Out) m.TurnSeat = NextSeat(m, m.TurnSeat);
        Event(m, "round", $"Round {m.Round}. Cups rattle. {m.TotalDice} dice on the table. {m.Players[m.TurnSeat].Name} bids first.");
    }

    private static void NextRound(BonesMatch m)
    {
        var alive = m.InPlay.ToList();
        if (alive.Count <= 1)
        {
            Finish(m, alive.FirstOrDefault()?.Seat ?? m.LastReveal!.CallerSeat);
            return;
        }
        m.Round++;
        StartRound(m);
    }

    private static void PlaceBid(BonesMatch m, int seat, int quantity, int face)
    {
        if (face is < 1 or > 6) throw new BonesRuleException("Pick a face from 1 to 6.");
        if (quantity < 1 || quantity > m.TotalDice) throw new BonesRuleException($"Bid between 1 and {m.TotalDice} dice.");
        if (m.CurrentBid is { } b && !(quantity > b.Quantity || (quantity == b.Quantity && face > b.Face)))
            throw new BonesRuleException($"Raise the bid: more than {b.Quantity}, or {b.Quantity} of a higher face than {FaceName(b.Face, b.Quantity)}.");

        var bid = new Bid(quantity, face, seat);
        m.CurrentBid = bid;
        m.RoundBids.Add(bid);
        var p = m.Players[seat];
        Event(m, "bid", $"{Who(p)} {(p.IsHuman ? "bid" : "bids")}: {quantity} {FaceName(face, quantity)}.", seat);
        BonesAi.MaybeShowTell(m, p, bid);
        m.TurnSeat = NextSeat(m, seat);
    }

    private static void Call(BonesMatch m, int callerSeat)
    {
        var bid = m.CurrentBid ?? throw new BonesRuleException("There is no bid to call yet — make the first bid.");
        var caller = m.Players[callerSeat];
        Event(m, "call", $"{Who(caller)} {(caller.IsHuman ? "call" : "calls")}: “Liar!”", callerSeat);

        var count = m.InPlay.Sum(p => p.Dice.Count(d => d == bid.Face || (bid.Face != BonesRules.WildFace && d == BonesRules.WildFace)));
        var truth = count >= bid.Quantity;
        var loser = truth ? caller : m.Players[bid.Seat];
        var dice = m.InPlay.ToDictionary(p => p.Seat, p => p.Dice.OrderBy(d => d).ToList());

        loser.Dice.RemoveAt(loser.Dice.Count - 1);
        m.LastReveal = new RevealReport(m.Round, bid, callerSeat, count, truth, loser.Seat, dice);

        Event(m, "reveal", $"The cups lift: {count} {FaceName(bid.Face, count)} on the table. {(truth ? "The bid was true." : "The bid was a lie.")} {Who(loser)} {(loser.IsHuman ? "lose" : "loses")} a die{(loser.Out ? " — and is out." : ".")}", loser.Seat);
        m.TurnSeat = loser.Out ? NextSeat(m, loser.Seat) : loser.Seat;
        m.Phase = BonesPhase.Reveal;
    }

    private static void Finish(BonesMatch m, int winner)
    {
        m.Phase = BonesPhase.Finished;
        m.WinnerSeat = winner;
        Event(m, "end", $"{Who(m.Players[winner])} {(m.Players[winner].IsHuman ? "take" : "takes")} the pot of {m.Pot} gold.", winner);
    }

    // ------------------------------------------------------------------ helpers

    internal static int NextSeat(BonesMatch m, int seat)
    {
        for (var i = 1; i <= m.Players.Count; i++)
        {
            var s = (seat + i) % m.Players.Count;
            if (!m.Players[s].Out) return s;
        }
        return seat;
    }

    private static void RequireTurn(BonesMatch m, int seat)
    {
        if (m.Phase != BonesPhase.Bidding) throw new BonesRuleException("Wait for the next round.");
        if (m.TurnSeat != seat) throw new BonesRuleException("It is not your turn.");
    }

    internal static void Event(BonesMatch m, string kind, string text, int? seat = null) => m.Events.Add(new BonesEvent(m.Round, kind, text, seat));

    private static string Who(BonesPlayer p) => p.IsHuman ? "You" : p.Name;

    public static string FaceName(int face, int quantity) => (face, quantity == 1) switch
    {
        (1, true) => "one (the Witch's eye)",
        (1, false) => "ones",
        (2, true) => "two",
        (2, false) => "twos",
        (3, true) => "three",
        (3, false) => "threes",
        (4, true) => "four",
        (4, false) => "fours",
        (5, true) => "five",
        (5, false) => "fives",
        (6, true) => "six",
        _ => "sixes",
    };
}
