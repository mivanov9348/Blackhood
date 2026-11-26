using Blackhood.Engine;
using Blackhood.Engine.Gifts;

namespace Blackhood.Engine.Tests;

internal static class Fixtures
{
    public static readonly TableHost Host = new("H01", "The Gentleman", "The Host", 15, false,
        "Welcome.", "Welcome back.", "The host.", "Let us begin.", "As you wish.", "Your role.");

    public static List<TableGuest> Guests(int count = 8) => Enumerable.Range(1, count).Select(i =>
        new TableGuest($"P{i:00}", $"Guest {i}", $"Title {i}", i * 40, false,
            new Traits(Insight: 1 + i % 5, Cunning: 1 + (i + 2) % 5, Composure: 1 + (i + 1) % 5, Greed: 1 + (i + 3) % 5, FearOfLoss: 1 + (i + 4) % 5),
            [new Tell(TellState.Lying, "Spins a coin", true), new Tell(TellState.Afraid, "Looks at the door", false)],
            "Hello.", null, $"Guest {i}.", "Again.", "Ah…", "Spins a coin")).ToList();

    /// <summary>A 5-player match with a human at seat 0, stopped at the first whisper.</summary>
    public static GiftsMatch HumanMatch(int seed = 1) =>
        GiftsGame.Create("You", seed, Host, Guests(), new HashSet<string>());

    /// <summary>Puts the match in a controlled state for a rule test.</summary>
    public static GiftsMatch Scenario(int candle = 2, int assassinSeat = 1)
    {
        var m = HumanMatch();
        m.Candle = candle;
        foreach (var p in m.Players)
        {
            p.Role = p.Seat == assassinSeat ? Role.Assassin : Role.Guest;
            p.Hand.Clear();
        }
        m.Phase = GiftsPhase.Delivery;
        return m;
    }

    public static Item Give(GiftsMatch m, int seat, ItemType type)
    {
        var item = GiftsGame.NewItem(m, type);
        m.Players[seat].Hand.Add(item);
        return item;
    }

    /// <summary>Sends a card gift and resolves it with the given response, bypassing the phase loop.</summary>
    public static Gift Deliver(GiftsMatch m, int from, int to, ItemType type, DeliveryResponse response = DeliveryResponse.Accept, Declaration declared = Declaration.Bread)
    {
        var card = GiftsGame.NewItem(m, type);
        var g = new Gift { Id = m.Gifts.Count + 1, Candle = m.Candle, From = from, To = to, Kind = GiftKind.Card, Card = card, Declared = declared, Announced = true };
        m.Gifts.Add(g);
        GiftsGame.ApplyResponse(m, g, response);
        return g;
    }
}
