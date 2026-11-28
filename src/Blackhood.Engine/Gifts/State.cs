namespace Blackhood.Engine.Gifts;

public enum GiftsPhase
{
    /// <summary>The host whispers offers; players may buy from him or play a Monocle.</summary>
    Whisper,
    /// <summary>Everyone secretly hands the host one gift.</summary>
    Gifts,
    /// <summary>The host announces gifts one by one; recipients may close the door or raise a mirror.</summary>
    Delivery,
    /// <summary>Whoever received Wine learns who sent one gift.</summary>
    WineChoice,
    /// <summary>Emergency amulet offers to whoever just received a dagger or poison.</summary>
    LastWhisper,
    /// <summary>Deaths, broken amulets and gold changes are revealed.</summary>
    Dawn,
    /// <summary>Hands above the limit are trimmed.</summary>
    Discard,
    /// <summary>After the sixth candle every living guest names the Assassin.</summary>
    Pointing,
    Finished,
}

public enum Winner
{
    None,
    Guests,
    Assassin,
}

public enum OfferKind
{
    // from the chest (limited, price set by the host)
    Amulet,
    Poison,
    Mirror,
    // fixed-price services
    Door,
    Rumor,
    LearnSender,
    Diversion,
    Peek,
}

public enum GiftKind
{
    Card,
    Purse,
    /// <summary>The host's free, unlimited Bread — an honest empty gift.</summary>
    FreeBread,
}

public enum GiftOutcome
{
    Pending,
    Accepted,
    DoorClosed,
    Mirrored,
}

public enum DeathCause
{
    Dagger,
    Poison,
}

public sealed class GiftsPlayer
{
    public int Seat { get; set; }
    public string Name { get; set; } = "";
    public bool IsHuman { get; set; }
    public TableGuest? Guest { get; set; }
    public Traits Traits { get; set; } = Traits.Average;
    public bool MetBefore { get; set; }
    public Role Role { get; set; }

    /// <summary>True gold. Only the player (and the host) know the live value.</summary>
    public int Gold { get; set; }
    /// <summary>What the table sees: updated at income and at dawn.</summary>
    public int PublicGold { get; set; }
    /// <summary>Change revealed at the last dawn (payments, purses) — the table sees how much, never why.</summary>
    public int LastDelta { get; set; }

    public List<Item> Hand { get; set; } = [];
    public bool Alive { get; set; } = true;
    public int? DiedCandle { get; set; }
    public DeathCause? DeathCause { get; set; }

    // ---- per-night state ----
    public bool LethalTonight { get; set; }
    public int? LethalFrom { get; set; }
    public int? PoisonDueCandle { get; set; }
    public int? PoisonedBy { get; set; }
    public bool DiversionTonight { get; set; }
    public int PendingWine { get; set; }
    public bool ReceivedDaggerTonight { get; set; }
    public bool ReceivedPoisonTonight { get; set; }
    public bool ReceivedDaggerLastNight { get; set; }
    public bool SentGiftTonight { get; set; }
    /// <summary>Ghosts point silently at one player each night ("Cold breath").</summary>
    public int? GhostPoint { get; set; }
    public int? FinalPoint { get; set; }

    // ---- what this player knows (used by the NPC AI and shown to the human as notes) ----
    public List<int> KnownInnocent { get; set; } = [];
    public int? KnownKiller { get; set; }
    /// <summary>Suspicion score per seat (NPC AI).</summary>
    public double[] Suspicion { get; set; } = [];
    /// <summary>Seats this player has seen holding an amulet (Monocle, peek, rumor).</summary>
    public List<int> SeenAmulet { get; set; } = [];
    /// <summary>Gift ids whose sender this player has learned (Wine, bribe).</summary>
    public List<int> KnownGiftSenders { get; set; } = [];
    /// <summary>Candles on which this player's own amulet broke.</summary>
    public List<int> AmuletBrokeCandles { get; set; } = [];
    /// <summary>Assassin memory: targets who just spent their amulet against one of my daggers.</summary>
    public List<int> Unprotected { get; set; } = [];

    public int Count(ItemType t) => Hand.Count(i => i.Type == t);
    public bool Has(ItemType t) => Hand.Any(i => i.Type == t);
}

public sealed class Gift
{
    public int Id { get; set; }
    public int Candle { get; set; }
    public int From { get; set; }
    public int To { get; set; }
    public GiftKind Kind { get; set; }
    public Item? Card { get; set; }
    public int Purse { get; set; }
    public Declaration Declared { get; set; }
    public GiftOutcome Outcome { get; set; }
    public bool Announced { get; set; }
    public bool Delivered { get; set; }
    /// <summary>Redirected by a Diversion bribe.</summary>
    public bool Diverted { get; set; }

    public bool IsHarmful => Card?.Type is ItemType.Dagger or ItemType.Poison;
}

public sealed class Offer
{
    public int Id { get; set; }
    public int Seat { get; set; }
    public int Candle { get; set; }
    public OfferKind Kind { get; set; }
    public int Price { get; set; }
    public string Text { get; set; } = "";
    /// <summary>Made after delivery to someone who just received a dagger or poison.</summary>
    public bool Emergency { get; set; }
    public bool Resolved { get; set; }
    public bool Accepted { get; set; }
}

/// <summary>Something everyone at the table saw or heard.</summary>
public sealed record GiftsEvent(int Candle, string Kind, string Text, int? Seat = null, int? Target = null);

/// <summary>Something only one player knows (whispers, wine, monocle, rumors…).</summary>
public sealed record PrivateNote(int Candle, int Seat, string Text);

public sealed record DawnReport(int Candle, List<int> Died, int AmuletsBroken, int? RichestSeat, Dictionary<int, int> GoldDeltas);

public sealed record FinalScore(int Seat, int Gold, bool PointedRight, int DaggerPenalty, int Score);

/// <summary>The full, authoritative state of one evening of Gifts. Never send this to a client — use <see cref="GiftsView"/>.</summary>
public sealed class GiftsMatch
{
    public Guid Id { get; set; }
    public int Seed { get; set; }
    public Rng Rng { get; set; } = new();
    public TableHost Host { get; set; } = null!;
    public bool HostMetBefore { get; set; }
    public bool IntroDone { get; set; }

    public int Candle { get; set; } = 1;
    public GiftsPhase Phase { get; set; } = GiftsPhase.Whisper;
    public int Quota { get; set; }
    public int Deaths { get; set; }
    public int EmergencyAmuletsSold { get; set; }

    public List<GiftsPlayer> Players { get; set; } = [];
    public List<Item> Deck { get; set; } = [];
    public List<Item> Discard { get; set; } = [];
    public Dictionary<ItemType, int> ChestStock { get; set; } = [];
    public int NextItemId { get; set; } = 1;

    public List<Gift> Gifts { get; set; } = [];
    public List<int> DeliveryOrder { get; set; } = [];
    public int DeliveryIndex { get; set; }
    /// <summary>The gift waiting for the human's door / mirror / accept decision.</summary>
    public int? AwaitingGiftId { get; set; }

    public List<Offer> Offers { get; set; } = [];
    public int NextOfferId { get; set; } = 1;

    public List<GiftsEvent> Events { get; set; } = [];
    public List<PrivateNote> Notes { get; set; } = [];
    public DawnReport? LastDawn { get; set; }

    // human pacing flags (reset each candle)
    public bool HumanWhisperDone { get; set; }
    public bool HumanGiftDone { get; set; }
    public bool HumanDawnSeen { get; set; }

    public Winner Winner { get; set; }
    public List<int> WinnerSeats { get; set; } = [];
    public int? SlayerSeat { get; set; }
    public List<FinalScore> Scores { get; set; } = [];

    public GiftsPlayer? Human => Players.FirstOrDefault(p => p.IsHuman);
    public IEnumerable<GiftsPlayer> Living => Players.Where(p => p.Alive);
    public IEnumerable<Gift> Tonight => Gifts.Where(g => g.Candle == Candle);
}
