using System.Text.Json;
using Blackhood.Engine.Gifts;

namespace Blackhood.Api.GameLogs;

/// <summary>What gets written to GameLogs for an evening of Gifts.</summary>
public static class GiftsLog
{
    public static void Start(GameLogWriter log, GiftsMatch m) =>
        log.Add(m.Candle, m.Phase.ToString(), "start",
            "Seats: " + string.Join(", ", m.Players.Select(p => $"{p.Seat} {p.Name} [{p.Role}]")) + $" · host {m.Host.Name} · seed {m.Seed}",
            new
            {
                m.Seed,
                Host = m.Host.Code,
                Players = m.Players.Select(p => new { p.Seat, p.Name, Code = p.Guest?.Code, p.Role, p.IsHuman, Hand = p.Hand.Select(i => i.Type) }),
            });

    public static void Action(GameLogWriter log, GiftsMatch m, int candleBefore, GiftsPhase phaseBefore, object action, string type) =>
        log.Add(candleBefore, phaseBefore.ToString(), "action", $"{type} ({phaseBefore} → {m.Phase})", action);

    public static void Error(GameLogWriter log, GiftsMatch m, object action, string message) =>
        log.Add(m.Candle, m.Phase.ToString(), "error", message, action);

    /// <summary>New public events and new private notes for the human since the move began.</summary>
    public static void Delta(GameLogWriter log, GiftsMatch m, int eventsBefore, int notesBefore)
    {
        var events = m.Events.Skip(eventsBefore).ToList();
        if (events.Count > 0)
            log.Add(m.Candle, m.Phase.ToString(), "events", string.Join("\n", events.Select(e => $"[c{e.Candle} {e.Kind}] {e.Text}")));

        var notes = m.Notes.Skip(notesBefore).Where(n => n.Seat == 0).ToList();
        if (notes.Count > 0)
            log.Add(m.Candle, m.Phase.ToString(), "notes", string.Join("\n", notes.Select(n => $"[c{n.Candle}] {n.Text}")));
    }

    /// <summary>The secret side of finished candles: who sent what to whom, and who bought what.</summary>
    public static void Nights(GameLogWriter log, GiftsMatch m, int fromCandle, int toCandle)
    {
        for (var c = fromCandle; c <= toCandle; c++)
        {
            var lines = new List<string>();
            foreach (var g in m.Gifts.Where(g => g.Candle == c))
            {
                var what = g.Kind switch
                {
                    GiftKind.Purse => $"Purse({g.Purse})",
                    GiftKind.FreeBread => "free Bread",
                    _ => g.Card!.Type.ToString(),
                };
                var lethal = g.Card?.Type == ItemType.Dagger && m.Players[g.From].Role == Role.Assassin && c >= GiftsRules.FirstLethalCandle ? " ☠" : "";
                lines.Add($"{m.Players[g.From].Name} → {m.Players[g.To].Name}: {what}{lethal} announced as {g.Declared} → {g.Outcome}{(g.Diverted ? " (diverted)" : "")}");
            }
            foreach (var o in m.Offers.Where(o => o.Candle == c && o.Accepted))
                lines.Add($"{m.Players[o.Seat].Name} bought {o.Kind} for {o.Price}{(o.Emergency ? " (emergency)" : "")}");
            if (lines.Count > 0)
                log.Add(c, "Night", "night", $"Candle {c} — secret summary:\n" + string.Join("\n", lines));
        }
    }

    public static void Result(GameLogWriter log, GiftsMatch m, int payout) =>
        log.Add(m.Candle, m.Phase.ToString(), "result",
            $"{m.Winner} win · deaths {m.Deaths}/{m.Quota} · candle {m.Candle} · assassin(s): {string.Join(", ", m.Players.Where(p => p.Role == Role.Assassin).Select(p => p.Name))} · winners: {string.Join(", ", m.WinnerSeats.Select(s => m.Players[s].Name))} · human {(m.Players[0].Alive ? "alive" : "dead")} as {m.Players[0].Role}, payout {payout}",
            new { m.Winner, m.Deaths, m.Quota, m.Candle, m.WinnerSeats, m.SlayerSeat, m.Scores, Roles = m.Players.Select(p => new { p.Name, p.Role, p.Alive }) });
}
