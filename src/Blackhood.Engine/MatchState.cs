namespace Blackhood.Engine;

public enum Phase
{
    Draw,
    Table,
    Night,
    Resolution,
    Whisper,
    Court,
    Finished,
}

public sealed class Seat
{
    public int Index { get; set; }
    public string Name { get; set; } = "";
    public bool IsHuman { get; set; }
    /// <summary>Snapshot of the character at match start (null for the human).</summary>
    public TableGuest? Guest { get; set; }
    /// <summary>The human player has sat with this character before.</summary>
    public bool MetBefore { get; set; }
    public RoleType Role { get; set; }
    public List<Card> Hand { get; set; } = [];
    /// <summary>Cards taken face-up from the market — public information.</summary>
    public List<Card> TakenFromMarket { get; set; } = [];
    public bool HasSoul { get; set; } = true;
    public bool IsAlive { get; set; } = true;
}

public sealed record LogEntry(int Round, string Text);

public sealed record Whisper(int Round, int ToSeat, string Text);

/// <summary>The full, authoritative state of one match. Never send this to a client — use <see cref="MatchView"/>.</summary>
public sealed class MatchState
{
    public const int TotalCandles = 6;
    public const int MarketSize = 3;
    public const int MaxHandSize = 5;
    public const int StartingHand = 3;

    public Guid Id { get; set; }
    public int Seed { get; set; }
    public TableHost Host { get; set; } = null!;
    public bool HostMetBefore { get; set; }
    /// <summary>The arrival scene has been watched (or skipped).</summary>
    public bool IntroDone { get; set; }
    public int Round { get; set; } = 1;
    public Phase Phase { get; set; } = Phase.Draw;
    public List<Seat> Seats { get; set; } = [];
    public List<Card> DrawPile { get; set; } = [];
    public List<Card> Market { get; set; } = [];
    public List<Card> Discard { get; set; } = [];
    public List<LogEntry> Log { get; set; } = [];
    public List<Whisper> Whispers { get; set; } = [];
}
