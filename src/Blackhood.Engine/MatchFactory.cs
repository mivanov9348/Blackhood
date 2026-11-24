namespace Blackhood.Engine;

public static class MatchFactory
{
    public const int Opponents = 4;

    /// <summary>
    /// Sets up a single-player match: the human at seat 0, four random guests from
    /// <paramref name="pool"/>, the given host, secret roles, 3 cards each and a 3-card market.
    /// </summary>
    /// <param name="metBefore">Character codes the player has already sat with.</param>
    public static MatchState CreateSingleplayer(
        string playerName,
        int seed,
        TableHost host,
        IReadOnlyList<TableGuest> pool,
        IReadOnlySet<string> metBefore)
    {
        if (pool.Count < Opponents)
            throw new InvalidOperationException($"Need at least {Opponents} published participants, found {pool.Count}.");

        var rng = new Random(seed);

        // Table composition — 4 random guests for now. The scoring algorithm from
        // "Съставяне на масата" (concept v0.4) replaces this later.
        var guests = pool.ToList();
        Deck.Shuffle(guests, rng);
        guests = guests.Take(Opponents).ToList();

        var roles = RoleSets.Prototype.ToList();
        Deck.Shuffle(roles, rng);

        var match = new MatchState
        {
            Id = Guid.NewGuid(),
            Seed = seed,
            Host = host,
            HostMetBefore = metBefore.Contains(host.Code),
        };

        match.Seats.Add(new Seat { Index = 0, Name = playerName, IsHuman = true, Role = roles[0] });
        for (var i = 0; i < guests.Count; i++)
            match.Seats.Add(new Seat
            {
                Index = i + 1,
                Name = guests[i].Name,
                Guest = guests[i],
                MetBefore = metBefore.Contains(guests[i].Code),
                Role = roles[i + 1],
            });

        var deck = Deck.CreateStandard();
        Deck.Shuffle(deck, rng);
        match.DrawPile = deck;

        for (var c = 0; c < MatchState.StartingHand; c++)
            foreach (var seat in match.Seats)
                seat.Hand.Add(Draw(match));

        for (var m = 0; m < MatchState.MarketSize; m++)
            match.Market.Add(Draw(match));

        match.Log.Add(new LogEntry(1, $"{host.Name} запали шест свещи. Масата е съставена."));
        foreach (var seat in match.Seats.Where(s => !s.IsHuman))
            match.Log.Add(new LogEntry(1, $"{seat.Name} седна на масата."));
        match.Log.Add(new LogEntry(1, "Всеки получи тайна роля и три карти."));

        match.Whispers.Add(new Whisper(1, 0, host.RoleDeal));

        return match;
    }

    private static Card Draw(MatchState match)
    {
        var card = match.DrawPile[^1];
        match.DrawPile.RemoveAt(match.DrawPile.Count - 1);
        return card;
    }
}
