namespace Blackhood.Engine;

/// <summary>What one seat is allowed to see. Hidden roles and hands of others never leave the server.</summary>
public sealed record MatchView(
    Guid Id,
    int Round,
    int TotalCandles,
    Phase Phase,
    bool IntroDone,
    HostView Host,
    SelfView Me,
    IReadOnlyList<OpponentView> Opponents,
    IReadOnlyList<Card> Market,
    int DeckCount,
    IReadOnlyList<LogEntry> Log,
    IReadOnlyList<string> Whispers)
{
    public static MatchView ForSeat(MatchState match, int seatIndex)
    {
        var me = match.Seats[seatIndex];
        var h = match.Host;

        var opponents = match.Seats
            .Where(s => s.Index != seatIndex)
            .Select(s => new OpponentView(
                s.Index,
                s.Guest?.Code ?? "",
                s.Name,
                s.Guest?.Title ?? "",
                s.Guest?.PortraitHue ?? 30,
                s.Guest?.HasPortrait ?? false,
                s.MetBefore,
                s.Hand.Count,
                s.TakenFromMarket.Select(c => c.Type).ToList(),
                s.IsAlive,
                s.IsAlive ? null : s.Role,
                s.Guest is { } g
                    ? new GuestIntro(g.HostIntroduction, s.MetBefore ? g.MetAgainLine : g.FirstLine, s.MetBefore ? null : g.FirstLineDirection, g.IntroGesture)
                    : null))
            .ToList();

        return new MatchView(
            match.Id,
            match.Round,
            MatchState.TotalCandles,
            match.Phase,
            match.IntroDone,
            new HostView(h.Code, h.Name, h.Title, h.PortraitHue, h.HasPortrait, match.HostMetBefore,
                new HostIntro(match.HostMetBefore ? h.MetAgainGreeting : h.Greeting, h.ReplyWho, h.ReplyBegin, h.ReplySilent, h.RoleDeal)),
            new SelfView(me.Index, me.Name, me.Role, me.Hand, me.HasSoul),
            opponents,
            match.Market,
            match.DrawPile.Count,
            match.Log,
            match.Whispers.Where(w => w.ToSeat == seatIndex).Select(w => w.Text).ToList());
    }
}

public sealed record HostView(string Code, string Name, string Title, int PortraitHue, bool HasPortrait, bool MetBefore, HostIntro Intro);

public sealed record HostIntro(string Greeting, string ReplyWho, string ReplyBegin, string ReplySilent, string RoleDeal);

public sealed record SelfView(int Seat, string Name, RoleType Role, IReadOnlyList<Card> Hand, bool HasSoul);

public sealed record OpponentView(
    int Seat,
    string Code,
    string Name,
    string Title,
    int PortraitHue,
    bool HasPortrait,
    bool MetBefore,
    int CardCount,
    IReadOnlyList<CardType> TakenFromMarket,
    bool IsAlive,
    RoleType? RevealedRole,
    GuestIntro? Intro);

/// <summary>Public lines for the arrival scene — nothing secret in here.</summary>
public sealed record GuestIntro(string HostIntroduction, string Line, string? Direction, string Gesture);
