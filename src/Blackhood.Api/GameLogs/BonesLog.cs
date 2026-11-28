using Blackhood.Engine.Bones;

namespace Blackhood.Api.GameLogs;

/// <summary>What gets written to GameLogs for a game of Bones.</summary>
public static class BonesLog
{
    public static void Start(GameLogWriter log, BonesMatch m) =>
        log.Add(m.Round, m.Phase.ToString(), "start",
            "Seats: " + string.Join(", ", m.Players.Select(p => $"{p.Seat} {p.Name}")) + $" · pot {m.Pot} · seed {m.Seed}",
            new { m.Seed, Players = m.Players.Select(p => new { p.Seat, p.Name, Code = p.Guest?.Code, p.IsHuman, p.Traits }) });

    public static void Action(GameLogWriter log, BonesMatch m, int roundBefore, BonesPhase phaseBefore, object action, string type, IReadOnlyList<int> myDice) =>
        log.Add(roundBefore, phaseBefore.ToString(), "action", $"{type} (my dice {string.Join(",", myDice)})", action);

    public static void Error(GameLogWriter log, BonesMatch m, object action, string message) =>
        log.Add(m.Round, m.Phase.ToString(), "error", message, action);

    public static void Delta(GameLogWriter log, BonesMatch m, int eventsBefore, int roundBefore)
    {
        var events = m.Events.Skip(eventsBefore).ToList();
        if (events.Count > 0)
            log.Add(m.Round, m.Phase.ToString(), "events", string.Join("\n", events.Select(e => $"[r{e.Round} {e.Kind}] {e.Text}")));

        // Every reveal shows all cups — log them so a round can be judged afterwards.
        if (m.LastReveal is { } r && r.Round >= roundBefore && events.Any(e => e.Kind == "reveal"))
            log.Add(r.Round, "Reveal", "reveal",
                $"Bid {r.Bid.Quantity}×{r.Bid.Face} by {m.Players[r.Bid.Seat].Name}, called by {m.Players[r.CallerSeat].Name}: {r.Count} on the table → {(r.BidWasTrue ? "true" : "lie")}, {m.Players[r.LoserSeat].Name} loses a die. Cups: "
                + string.Join(" | ", r.Dice.Select(kv => $"{m.Players[kv.Key].Name} {string.Join(",", kv.Value)}")),
                r);
    }

    public static void Result(GameLogWriter log, BonesMatch m, int payout) =>
        log.Add(m.Round, m.Phase.ToString(), "result",
            $"Winner {m.Players[m.WinnerSeat!.Value].Name} after {m.Round} rounds · human {(m.WinnerSeat == 0 ? "won" : "lost")}, payout {payout}",
            new { m.WinnerSeat, m.Round, m.Pot });
}
