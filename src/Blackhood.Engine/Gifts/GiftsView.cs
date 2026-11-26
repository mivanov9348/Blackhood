namespace Blackhood.Engine.Gifts;

/// <summary>What one seat may see. Other players' roles, hands and true gold never leave the server.</summary>
public sealed record GiftsView(
    Guid Id,
    int Candle,
    int TotalCandles,
    GiftsPhase Phase,
    bool IntroDone,
    int Quota,
    int Deaths,
    HostView Host,
    SelfView Me,
    IReadOnlyList<SeatView> Players,
    ChestView Chest,
    int DeckCount,
    IReadOnlyList<OfferView> Offers,
    IReadOnlyList<ServiceView> Services,
    IReadOnlyList<GiftsEvent> Events,
    IReadOnlyList<PrivateNote> Notes,
    PendingDeliveryView? Pending,
    IReadOnlyList<AnnouncedGiftView> TonightGifts,
    IReadOnlyList<MyGiftView> MyLastNightGifts,
    int DiscardCount,
    DawnReport? Dawn,
    ResultView? Result)
{
    public static GiftsView ForSeat(GiftsMatch m, int seat)
    {
        var me = m.Players[seat];
        var finished = m.Phase == GiftsPhase.Finished;
        var h = m.Host;

        var players = m.Players.Select(p =>
        {
            var self = p.Seat == seat;
            var revealed = finished || !p.Alive || self;
            return new SeatView(
                p.Seat,
                p.Guest?.Code ?? "",
                p.Name,
                p.Guest?.Title ?? (p.IsHuman ? "You" : ""),
                p.Guest?.PortraitHue ?? 30,
                p.Guest?.HasPortrait ?? false,
                p.MetBefore,
                self,
                p.Alive,
                self ? p.Gold : p.PublicGold,
                p.LastDelta,
                p.Hand.Count,
                revealed ? p.Role : null,
                !p.Alive || finished ? p.Hand.Select(i => i.Type).ToList() : null,
                p.GhostPoint,
                p.DeathCause,
                p.Guest is { } g
                    ? new GuestIntro(g.HostIntroduction, p.MetBefore ? g.MetAgainLine : g.FirstLine, p.MetBefore ? null : g.FirstLineDirection, g.IntroGesture)
                    : null);
        }).ToList();

        var offers = m.Offers
            .Where(o => o.Seat == seat && o.Candle == m.Candle && !o.Resolved &&
                        (o.Emergency ? m.Phase == GiftsPhase.LastWhisper : m.Phase == GiftsPhase.Whisper))
            .Select(o => new OfferView(o.Id, o.Kind, o.Price, o.Text, o.Emergency))
            .ToList();

        PendingDeliveryView? pending = null;
        if (m.AwaitingGiftId is { } gid && m.Gifts.First(g => g.Id == gid) is { } pg && pg.To == seat)
            pending = new PendingDeliveryView(pg.Id, pg.Declared, me.Has(ItemType.ClosedDoor), me.Has(ItemType.Mirror));

        var tonight = m.Tonight.Where(g => g.Announced)
            .OrderBy(g => m.DeliveryOrder.IndexOf(g.Id))
            .Select(g => new AnnouncedGiftView(g.Id, g.To, g.Declared, g.Delivered ? g.Outcome : GiftOutcome.Pending, g.From == seat))
            .ToList();

        var lastNight = m.Gifts.Where(g => g.Candle == m.Candle - 1 && g.To == seat && g.Delivered)
            .Select(g => new MyGiftView(g.Id, g.Declared, g.Outcome, g.Outcome == GiftOutcome.Accepted ? Describe(g) : "returned unopened"))
            .ToList();

        ResultView? result = null;
        if (finished)
            result = new ResultView(m.Winner, m.WinnerSeats, m.Scores, m.SlayerSeat,
                m.WinnerSeats.Contains(seat), me.Alive);

        return new GiftsView(
            m.Id,
            m.Candle,
            GiftsRules.Candles,
            m.Phase,
            m.IntroDone,
            m.Quota,
            m.Deaths,
            new HostView(h.Code, h.Name, h.Title, h.PortraitHue, h.HasPortrait, m.HostMetBefore,
                new HostIntro(m.HostMetBefore ? h.MetAgainGreeting : h.Greeting, h.ReplyWho, h.ReplyBegin, h.ReplySilent, h.RoleDeal)),
            new SelfView(seat, me.Name, me.Role, me.Gold, me.Hand, me.Alive, me.PoisonDueCandle is not null, me.SentGiftTonight || (m.HumanGiftDone && me.IsHuman), me.PendingWine, me.FinalPoint),
            players,
            new ChestView(m.ChestStock[ItemType.Amulet], m.ChestStock[ItemType.Poison], m.ChestStock[ItemType.Mirror]),
            m.Deck.Count,
            offers,
            m.Phase == GiftsPhase.Whisper && me.Alive ? FixedServices : [],
            m.Events,
            m.Notes.Where(n => n.Seat == seat).ToList(),
            pending,
            tonight,
            lastNight,
            m.Phase == GiftsPhase.Discard && me.Alive ? Math.Max(0, me.Hand.Count - GiftsRules.HandLimit) : 0,
            m.LastDawn,
            result);
    }

    private static string Describe(Gift g) => g.Kind switch
    {
        GiftKind.Purse => $"a purse of {g.Purse} gold",
        GiftKind.FreeBread => "bread",
        _ => GiftsGame.Article(g.Card!.Type),
    };

    private static readonly IReadOnlyList<ServiceView> FixedServices =
    [
        new(OfferKind.Door, GiftsRules.PriceDoor, "Buy a Closed Door", false, false),
        new(OfferKind.Rumor, GiftsRules.PriceRumor, "A rumor: one true thing about a player's hand", true, false),
        new(OfferKind.Peek, GiftsRules.PricePeek, "See a player's whole hand", true, false),
        new(OfferKind.LearnSender, GiftsRules.PriceLearnSender, "Learn who sent you one of last night's gifts", false, true),
        new(OfferKind.Diversion, GiftsRules.PriceDiversion, "Tonight one gift meant for you goes to someone else", false, false),
    ];
}

public sealed record HostView(string Code, string Name, string Title, int PortraitHue, bool HasPortrait, bool MetBefore, HostIntro Intro);

public sealed record HostIntro(string Greeting, string ReplyWho, string ReplyBegin, string ReplySilent, string RoleDeal);

/// <summary>Public lines for the arrival scene — nothing secret in here.</summary>
public sealed record GuestIntro(string HostIntroduction, string Line, string? Direction, string Gesture);

public sealed record SelfView(int Seat, string Name, Role Role, int Gold, IReadOnlyList<Item> Hand, bool Alive, bool Poisoned, bool GiftSent, int PendingWine, int? FinalPoint);

public sealed record SeatView(
    int Seat,
    string Code,
    string Name,
    string Title,
    int PortraitHue,
    bool HasPortrait,
    bool MetBefore,
    bool IsMe,
    bool Alive,
    int Gold,
    int LastDelta,
    int HandCount,
    Role? Role,
    IReadOnlyList<ItemType>? RevealedHand,
    int? GhostPoint,
    DeathCause? DeathCause,
    GuestIntro? Intro);

public sealed record ChestView(int Amulets, int Poisons, int Mirrors);

public sealed record OfferView(int Id, OfferKind Kind, int Price, string Text, bool Emergency);

public sealed record ServiceView(OfferKind Kind, int Price, string Text, bool NeedsTarget, bool NeedsGift);

public sealed record PendingDeliveryView(int GiftId, Declaration Declared, bool CanCloseDoor, bool CanMirror);

public sealed record AnnouncedGiftView(int Id, int To, Declaration Declared, GiftOutcome Outcome, bool SentByMe);

public sealed record MyGiftView(int Id, Declaration Declared, GiftOutcome Outcome, string Contents);

public sealed record ResultView(Winner Winner, IReadOnlyList<int> WinnerSeats, IReadOnlyList<FinalScore> Scores, int? SlayerSeat, bool YouWon, bool YouSurvived);
