namespace Blackhood.Engine.Gifts;

/// <summary>A move that breaks the rules (wrong phase, not your card, not enough gold…).</summary>
public sealed class GiftsRuleException(string message) : Exception(message);

public enum DeliveryResponse
{
    Accept,
    CloseDoor,
    Mirror,
}

/// <summary>
/// The rules of "Gifts". Every method mutates a <see cref="GiftsMatch"/>; NPC seats are
/// played by <see cref="GiftsAi"/>, the human seat waits for actions. After each human
/// action <see cref="Run"/> advances the evening until a human decision is needed again.
/// A match without a human seat plays itself to the end (used by the simulator).
/// </summary>
public static class GiftsGame
{
    // =====================================================================================
    // setup
    // =====================================================================================

    public static GiftsMatch Create(
        string? humanName,
        int seed,
        TableHost host,
        IReadOnlyList<TableGuest> pool,
        IReadOnlySet<string> metBefore,
        int players = 5)
    {
        if (players is < 5 or > 8) throw new ArgumentOutOfRangeException(nameof(players), "Gifts is played by 5–8 people.");
        var npcCount = humanName is null ? players : players - 1;
        if (pool.Count < npcCount) throw new InvalidOperationException($"Need at least {npcCount} published guests, found {pool.Count}.");

        var rng = new Rng(seed);
        var m = new GiftsMatch
        {
            Id = Guid.NewGuid(),
            Seed = seed,
            Rng = rng,
            Host = host,
            HostMetBefore = metBefore.Contains(host.Code),
            Quota = GiftsRules.Quota(players),
            ChestStock = GiftsRules.Chest.ToDictionary(kv => kv.Key, kv => kv.Value),
        };

        if (humanName is not null)
            m.Players.Add(new GiftsPlayer { Seat = 0, Name = humanName, IsHuman = true });

        var guests = pool.ToList();
        rng.Shuffle(guests);
        foreach (var g in guests.Take(npcCount))
            m.Players.Add(new GiftsPlayer { Seat = m.Players.Count, Name = g.Name, Guest = g, Traits = g.Traits, MetBefore = metBefore.Contains(g.Code) });

        // secret roles
        var seats = m.Players.Select(p => p.Seat).ToList();
        rng.Shuffle(seats);
        foreach (var seat in seats.Take(GiftsRules.Assassins(players)))
            m.Players[seat].Role = Role.Assassin;

        foreach (var p in m.Players)
        {
            p.Gold = p.PublicGold = GiftsRules.StartingGold;
            p.Suspicion = new double[players];
        }

        // the deck
        foreach (var (type, count) in GiftsRules.Deck)
            for (var i = 0; i < count; i++)
                m.Deck.Add(NewItem(m, type));
        rng.Shuffle(m.Deck);

        // Everyone starts with the same number of cards — a bigger hand would give the Assassin away.
        // The Assassin's two daggers take the place of two of his dealt cards.
        foreach (var p in m.Players)
        {
            var dealt = p.Role == Role.Assassin ? GiftsRules.StartingHand - GiftsRules.AssassinExtraDaggers : GiftsRules.StartingHand;
            for (var c = 0; c < dealt; c++) p.Hand.Add(DrawCard(m));
        }

        var assassins = m.Players.Where(p => p.Role == Role.Assassin).ToList();
        foreach (var a in assassins)
        {
            for (var i = 0; i < GiftsRules.AssassinExtraDaggers; i++)
                a.Hand.Add(NewItem(m, ItemType.Dagger));
            Note(m, a.Seat, assassins.Count > 1
                ? $"You are an Assassin. Your partner: {string.Join(", ", assassins.Where(x => x != a).Select(x => x.Name))}."
                : "You are the Assassin. Two extra daggers rest in your hand.");
        }
        foreach (var p in m.Players.Where(p => p.Role == Role.Guest))
            Note(m, p.Seat, "You are a Guest.");

        GiftsAi.InitBeliefs(m);

        Event(m, "host", $"{host.Name} lights six candles. The Assassin needs {m.Quota} dead before the last one goes out.");
        StartCandle(m);
        Run(m);
        return m;
    }

    // =====================================================================================
    // the main loop
    // =====================================================================================

    /// <summary>Advances the evening until a human decision is needed or the evening ends.</summary>
    public static void Run(GiftsMatch m)
    {
        for (var guard = 0; guard < 10_000; guard++)
        {
            var human = m.Human;
            switch (m.Phase)
            {
                case GiftsPhase.Whisper:
                    if (human is { Alive: true } && !m.HumanWhisperDone) return;
                    foreach (var p in m.Living.Where(p => !p.IsHuman)) GiftsAi.Whisper(m, p);
                    m.Phase = GiftsPhase.Gifts;
                    break;

                case GiftsPhase.Gifts:
                    if (human is not null && !m.HumanGiftDone) return;
                    foreach (var p in m.Living.Where(p => !p.IsHuman && !p.SentGiftTonight)) GiftsAi.SendGift(m, p);
                    foreach (var ghost in m.Players.Where(p => !p.Alive && !p.IsHuman)) GhostPoint(m, ghost, GiftsAi.GhostPoint(m, ghost));
                    BuildDelivery(m);
                    m.Phase = GiftsPhase.Delivery;
                    break;

                case GiftsPhase.Delivery:
                    if (!ProcessDelivery(m)) return;
                    m.Phase = GiftsPhase.WineChoice;
                    break;

                case GiftsPhase.WineChoice:
                    foreach (var p in m.Living.Where(p => !p.IsHuman && p.PendingWine > 0))
                        while (p.PendingWine > 0) RevealSender(m, p, GiftsAi.ChooseWine(m, p), "wine");
                    if (human is { PendingWine: > 0 }) return;
                    MakeEmergencyOffers(m);
                    m.Phase = GiftsPhase.LastWhisper;
                    break;

                case GiftsPhase.LastWhisper:
                    foreach (var o in m.Offers.Where(o => o is { Emergency: true, Resolved: false } && !m.Players[o.Seat].IsHuman))
                        ResolveOffer(m, o, GiftsAi.AcceptEmergency(m, m.Players[o.Seat], o));
                    if (m.Offers.Any(o => o is { Emergency: true, Resolved: false })) return;
                    Dawn(m);
                    break;

                case GiftsPhase.Dawn:
                    if (human is not null && !m.HumanDawnSeen) return;
                    foreach (var p in m.Living.Where(p => !p.IsHuman)) GiftsAi.TrimHand(m, p);
                    m.Phase = GiftsPhase.Discard;
                    break;

                case GiftsPhase.Discard:
                    if (human is { Alive: true } && human.Hand.Count > GiftsRules.HandLimit) return;
                    m.Candle++;
                    StartCandle(m);
                    break;

                case GiftsPhase.Pointing:
                    foreach (var p in m.Living.Where(p => !p.IsHuman && p.Role == Role.Guest && p.FinalPoint is null))
                        p.FinalPoint = GiftsAi.FinalPoint(m, p);
                    if (human is { Alive: true, Role: Role.Guest, FinalPoint: null }) return;
                    Finish(m, Winner.Guests);
                    return;

                case GiftsPhase.Finished:
                    return;
            }
        }
        throw new InvalidOperationException("Gifts loop did not settle.");
    }

    private static void StartCandle(GiftsMatch m)
    {
        m.HumanWhisperDone = m.HumanGiftDone = m.HumanDawnSeen = false;
        m.DeliveryOrder.Clear();
        m.DeliveryIndex = 0;
        m.AwaitingGiftId = null;

        foreach (var p in m.Players)
        {
            p.ReceivedDaggerLastNight = p.ReceivedDaggerTonight;
            p.ReceivedDaggerTonight = p.ReceivedPoisonTonight = p.LethalTonight = p.DiversionTonight = p.SentGiftTonight = false;
            p.LethalFrom = null;
            p.PendingWine = 0;
            p.GhostPoint = null;
            p.LastDelta = 0;
        }

        foreach (var p in m.Living)
        {
            p.Gold += GiftsRules.IncomePerCandle;
            p.PublicGold += GiftsRules.IncomePerCandle;
            p.Hand.Add(DrawCard(m));
        }

        Event(m, "candle", m.Candle == 1
            ? "The first candle is lit. Tonight is for getting acquainted — no dagger kills before the second candle."
            : $"Candle {m.Candle} is lit.");

        foreach (var p in m.Living) GiftsHost.MakeOffers(m, p);
        m.Phase = GiftsPhase.Whisper;
    }

    // =====================================================================================
    // whisper: offers, services, monocle
    // =====================================================================================

    public static void AcceptOffer(GiftsMatch m, int seat, int offerId, bool accept = true)
    {
        var o = m.Offers.FirstOrDefault(x => x.Id == offerId && x.Seat == seat && !x.Resolved)
                ?? throw new GiftsRuleException("That offer is no longer on the table.");
        if (o.Emergency ? m.Phase != GiftsPhase.LastWhisper : m.Phase != GiftsPhase.Whisper)
            throw new GiftsRuleException("The host is not taking offers right now.");
        ResolveOffer(m, o, accept);
        if (o.Emergency) Run(m);
    }

    private static void ResolveOffer(GiftsMatch m, Offer o, bool accept)
    {
        var p = m.Players[o.Seat];
        if (accept && p.Gold < o.Price) throw new GiftsRuleException($"You need {o.Price} gold.");
        o.Resolved = true;
        if (!accept) return;

        var item = o.Kind switch
        {
            OfferKind.Amulet => ItemType.Amulet,
            OfferKind.Poison => ItemType.Poison,
            OfferKind.Mirror => ItemType.Mirror,
            OfferKind.Door => ItemType.ClosedDoor,
            _ => throw new GiftsRuleException("Unknown offer."),
        };
        if (GiftsRules.Chest.ContainsKey(item))
        {
            if (m.ChestStock[item] <= 0)
            {
                // someone else bought the last one first
                Note(m, p.Seat, $"The host spreads his gloved hands: the chest holds no more {GiftsRules.Name(item).ToLowerInvariant()}s.");
                return;
            }
            m.ChestStock[item]--;
        }

        p.Gold -= o.Price;
        p.Hand.Add(NewItem(m, item));
        o.Accepted = true;
        Note(m, p.Seat, $"You paid {o.Price} gold. The host slips you {Article(item)}.");
    }

    public static void BuyService(GiftsMatch m, int seat, OfferKind kind, int? target = null, int? giftId = null)
    {
        Require(m, GiftsPhase.Whisper);
        var p = Alive(m, seat);
        var price = kind switch
        {
            OfferKind.Door => GiftsRules.PriceDoor,
            OfferKind.Rumor => GiftsRules.PriceRumor,
            OfferKind.LearnSender => GiftsRules.PriceLearnSender,
            OfferKind.Diversion => GiftsRules.PriceDiversion,
            OfferKind.Peek => GiftsRules.PricePeek,
            _ => throw new GiftsRuleException("The host does not sell that for a fixed price."),
        };
        if (p.Gold < price) throw new GiftsRuleException($"You need {price} gold.");

        switch (kind)
        {
            case OfferKind.Door:
                p.Hand.Add(NewItem(m, ItemType.ClosedDoor));
                Note(m, seat, "You bought a Closed Door.");
                break;
            case OfferKind.Rumor:
                var rt = Other(m, seat, target);
                Note(m, seat, $"Rumor: {GiftsHost.Rumor(m, rt)}");
                GiftsAi.OnSawHand(m, p, rt, partial: true);
                break;
            case OfferKind.Peek:
                var pt = Other(m, seat, target);
                Note(m, seat, $"The host shows you {pt.Name}'s hand: {HandText(pt)}.");
                GiftsAi.OnSawHand(m, p, pt, partial: false);
                break;
            case OfferKind.LearnSender:
                var g = m.Gifts.FirstOrDefault(x => x.Id == giftId && x.Candle == m.Candle - 1 && x.To == seat)
                        ?? throw new GiftsRuleException("Choose one of the gifts you received last night.");
                RevealSender(m, p, g, "bribe");
                break;
            case OfferKind.Diversion:
                if (p.DiversionTonight) throw new GiftsRuleException("You already paid for a diversion tonight.");
                p.DiversionTonight = true;
                Note(m, seat, "Tonight one gift meant for you will find someone else.");
                break;
        }
        p.Gold -= price;
    }

    public static void PlayMonocle(GiftsMatch m, int seat, int itemId, int target)
    {
        if (m.Phase is not (GiftsPhase.Whisper or GiftsPhase.Gifts)) throw new GiftsRuleException("Play the Monocle before the gifts are sent.");
        var p = Alive(m, seat);
        var card = p.Hand.FirstOrDefault(i => i.Id == itemId && i.Type == ItemType.Monocle) ?? throw new GiftsRuleException("You have no such Monocle.");
        var t = Other(m, seat, target);
        p.Hand.Remove(card);
        m.Discard.Add(card);
        Note(m, seat, $"Through the monocle you see {t.Name}'s hand: {HandText(t)}.");
        GiftsAi.OnSawHand(m, p, t, partial: false);
    }

    public static void EndWhisper(GiftsMatch m, int seat)
    {
        Require(m, GiftsPhase.Whisper);
        if (!m.Players[seat].IsHuman) throw new GiftsRuleException("Not your seat.");
        foreach (var o in m.Offers.Where(o => o.Seat == seat && o.Candle == m.Candle && !o.Emergency && !o.Resolved)) o.Resolved = true;
        m.HumanWhisperDone = true;
        Run(m);
    }

    // =====================================================================================
    // gifts
    // =====================================================================================

    public static void SendGift(GiftsMatch m, int seat, GiftKind kind, int to, Declaration declared, int? itemId = null, int purse = 0)
    {
        Require(m, GiftsPhase.Gifts);
        var p = Alive(m, seat);
        if (p.SentGiftTonight) throw new GiftsRuleException("You have already sent your gift tonight.");
        var r = Other(m, seat, to);

        var gift = new Gift { Id = m.Gifts.Count + 1, Candle = m.Candle, From = seat, To = r.Seat, Kind = kind, Declared = declared };
        switch (kind)
        {
            case GiftKind.Card:
                var card = p.Hand.FirstOrDefault(i => i.Id == itemId) ?? throw new GiftsRuleException("That card is not in your hand.");
                p.Hand.Remove(card);
                gift.Card = card;
                break;
            case GiftKind.Purse:
                if (purse < 1 || purse > p.Gold) throw new GiftsRuleException("Put between 1 gold and everything you have in the purse.");
                p.Gold -= purse;
                gift.Purse = purse;
                break;
            case GiftKind.FreeBread:
                break;
        }

        m.Gifts.Add(gift);
        p.SentGiftTonight = true;
        if (p.IsHuman) m.HumanGiftDone = true;
        GiftsAi.MaybeShowLyingTell(m, p, gift);
        if (p.IsHuman) Run(m);
    }

    /// <summary>A dead human skips sending (optionally pointing as a ghost).</summary>
    public static void GhostAct(GiftsMatch m, int seat, int? point)
    {
        Require(m, GiftsPhase.Gifts);
        var p = m.Players[seat];
        if (p.Alive || !p.IsHuman) throw new GiftsRuleException("Only a ghost can do that.");
        if (point is not null) GhostPoint(m, p, point);
        m.HumanGiftDone = true;
        Run(m);
    }

    private static void GhostPoint(GiftsMatch m, GiftsPlayer ghost, int? target)
    {
        if (target is not { } t || t == ghost.Seat || !m.Players[t].Alive) return;
        ghost.GhostPoint = t;
        Event(m, "ghost", $"A cold breath. The ghost of {ghost.Name} points at {m.Players[t].Name}.", ghost.Seat, t);
        GiftsAi.OnGhostPoint(m, ghost, t);
    }

    // =====================================================================================
    // delivery
    // =====================================================================================

    private static void BuildDelivery(GiftsMatch m)
    {
        var tonight = m.Tonight.ToList();
        m.Rng.Shuffle(tonight);

        // A Diversion sends the first gift meant for you to someone random.
        foreach (var p in m.Living.Where(p => p.DiversionTonight))
        {
            var g = tonight.FirstOrDefault(x => x.To == p.Seat && !x.Diverted);
            if (g is null) continue;
            var others = m.Living.Where(x => x.Seat != p.Seat && x.Seat != g.From).ToList();
            if (others.Count == 0) continue;
            g.To = m.Rng.Pick(others).Seat;
            g.Diverted = true;
        }

        m.DeliveryOrder = tonight.Select(g => g.Id).ToList();
        m.DeliveryIndex = 0;
        Event(m, "delivery", "The host gathers the gifts, shuffles them, and begins to deliver.");
    }

    /// <summary>Returns false while waiting for the human's decision.</summary>
    private static bool ProcessDelivery(GiftsMatch m)
    {
        while (m.DeliveryIndex < m.DeliveryOrder.Count)
        {
            var g = GiftById(m, m.DeliveryOrder[m.DeliveryIndex]);
            var r = m.Players[g.To];

            if (!g.Announced)
            {
                g.Announced = true;
                Event(m, "announce", $"{r.Name}, someone sends you {GiftsRules.Name(g.Declared)}.", null, r.Seat);
            }

            var canBlock = r.Has(ItemType.ClosedDoor) || r.Has(ItemType.Mirror);
            DeliveryResponse response;
            if (!canBlock) response = DeliveryResponse.Accept;
            else if (r.IsHuman)
            {
                m.AwaitingGiftId = g.Id;
                return false;
            }
            else response = GiftsAi.Respond(m, r, g);

            ApplyResponse(m, g, response);
            m.DeliveryIndex++;
        }
        return true;
    }

    public static void RespondToDelivery(GiftsMatch m, int seat, DeliveryResponse response)
    {
        Require(m, GiftsPhase.Delivery);
        var g = m.AwaitingGiftId is { } id ? GiftById(m, id) : throw new GiftsRuleException("No gift is waiting for you.");
        if (g.To != seat) throw new GiftsRuleException("That gift is not for you.");
        ApplyResponse(m, g, response);
        m.AwaitingGiftId = null;
        m.DeliveryIndex++;
        Run(m);
    }

    internal static void ApplyResponse(GiftsMatch m, Gift g, DeliveryResponse response)
    {
        var r = m.Players[g.To];
        var s = m.Players[g.From];
        g.Delivered = true;

        switch (response)
        {
            case DeliveryResponse.CloseDoor:
                var door = r.Hand.FirstOrDefault(i => i.Type == ItemType.ClosedDoor) ?? throw new GiftsRuleException("You have no Closed Door.");
                r.Hand.Remove(door);
                m.Discard.Add(door);
                g.Outcome = GiftOutcome.DoorClosed;
                ReturnToSender(m, g, s);
                Event(m, "door", $"{r.Name} closes the door. The gift goes back unopened.", r.Seat);
                Note(m, s.Seat, $"Your gift to {r.Name} came back unopened.");
                break;

            case DeliveryResponse.Mirror:
                var mirror = r.Hand.FirstOrDefault(i => i.Type == ItemType.Mirror) ?? throw new GiftsRuleException("You have no Mirror.");
                r.Hand.Remove(mirror); // a mirror shatters — out of the game
                g.Outcome = GiftOutcome.Mirrored;
                Event(m, "mirror", $"{r.Name} raises a mirror. The gift turns back on whoever sent it.", r.Seat);
                Receive(m, g, receiver: s, sender: s, mirroredBy: r);
                break;

            default:
                g.Outcome = GiftOutcome.Accepted;
                Receive(m, g, receiver: r, sender: s, mirroredBy: null);
                break;
        }
    }

    private static void Receive(GiftsMatch m, Gift g, GiftsPlayer receiver, GiftsPlayer sender, GiftsPlayer? mirroredBy)
    {
        var mirrored = mirroredBy is not null;
        switch (g.Kind)
        {
            case GiftKind.Purse:
                receiver.Gold += g.Purse;
                Note(m, receiver.Seat, mirrored ? $"Your purse of {g.Purse} gold came back to you." : $"You received a purse with {g.Purse} gold.");
                break;
            case GiftKind.FreeBread:
                Note(m, receiver.Seat, mirrored ? "Your bread came back to you." : "You received bread.");
                break;
            case GiftKind.Card:
                var card = g.Card!;
                receiver.Hand.Add(card);
                var lethalCandle = m.Candle >= GiftsRules.FirstLethalCandle;
                if (card.Type == ItemType.Dagger)
                {
                    // An Assassin's dagger kills a guest. Reflected by a mirror, it finds the Assassin's own heart.
                    var lethal = lethalCandle && sender.Role == Role.Assassin && (mirrored || receiver.Role != Role.Assassin);
                    if (lethal)
                    {
                        receiver.LethalTonight = true;
                        receiver.LethalFrom = mirroredBy?.Seat ?? sender.Seat;
                    }
                    receiver.ReceivedDaggerTonight = true;
                    GiftsAi.MaybeShowAfraidTell(m, receiver);
                }
                else if (card.Type == ItemType.Poison)
                {
                    receiver.PoisonDueCandle ??= m.Candle + 1;
                    receiver.PoisonedBy = mirroredBy?.Seat ?? sender.Seat;
                    receiver.ReceivedPoisonTonight = true;
                    GiftsAi.MaybeShowAfraidTell(m, receiver);
                }
                else if (card.Type == ItemType.Wine && !mirrored)
                {
                    receiver.PendingWine++;
                }

                var what = Article(card.Type);
                Note(m, receiver.Seat, mirrored
                    ? $"The mirror turned your gift back on you: {what}."
                    : $"You received {what} (announced as {GiftsRules.Name(g.Declared)}).{(card.Type == ItemType.Poison ? " It takes effect at the next dawn." : "")}");
                break;
        }
        GiftsAi.OnReceived(m, receiver, g, mirrored);
    }

    private static void ReturnToSender(GiftsMatch m, Gift g, GiftsPlayer sender)
    {
        switch (g.Kind)
        {
            case GiftKind.Card: sender.Hand.Add(g.Card!); break;
            case GiftKind.Purse: sender.Gold += g.Purse; break;
        }
    }

    // =====================================================================================
    // wine & the last whisper
    // =====================================================================================

    public static void ChooseWine(GiftsMatch m, int seat, int giftId)
    {
        Require(m, GiftsPhase.WineChoice);
        var p = m.Players[seat];
        if (p.PendingWine <= 0) throw new GiftsRuleException("You have no wine to drink.");
        var g = m.Tonight.FirstOrDefault(x => x.Id == giftId && x.Announced) ?? throw new GiftsRuleException("Choose one of tonight's gifts.");
        RevealSender(m, p, g, "wine");
        Run(m);
    }

    private static void RevealSender(GiftsMatch m, GiftsPlayer p, Gift? g, string how)
    {
        if (how == "wine") p.PendingWine = Math.Max(0, p.PendingWine - 1);
        if (g is null) return;
        var to = m.Players[g.To].Name;
        var from = m.Players[g.From].Name;
        Note(m, p.Seat, how == "wine"
            ? $"The wine loosens tongues: the gift to {to}, announced as {GiftsRules.Name(g.Declared)}, came from {from}."
            : $"The host leans close: the gift to {to}, announced as {GiftsRules.Name(g.Declared)}, came from {from}.");
        GiftsAi.OnLearnedSender(m, p, g);
    }

    private static void MakeEmergencyOffers(GiftsMatch m)
    {
        foreach (var p in m.Living.Where(p => (p.ReceivedDaggerTonight || p.ReceivedPoisonTonight) && !p.Has(ItemType.Amulet)))
            GiftsHost.EmergencyOffer(m, p);
    }

    // =====================================================================================
    // dawn
    // =====================================================================================

    public static void SeeDawn(GiftsMatch m, int seat)
    {
        Require(m, GiftsPhase.Dawn);
        if (!m.Players[seat].IsHuman) throw new GiftsRuleException("Not your seat.");
        m.HumanDawnSeen = true;
        Run(m);
    }

    internal static void Dawn(GiftsMatch m)
    {
        var died = new List<int>();
        var broken = 0;

        foreach (var p in m.Living.ToList())
        {
            var threats = new List<(DeathCause Cause, int? From)>();
            if (p.LethalTonight) threats.Add((DeathCause.Dagger, p.LethalFrom));
            if (p.PoisonDueCandle == m.Candle) threats.Add((DeathCause.Poison, p.PoisonedBy));
            if (p.PoisonDueCandle == m.Candle) p.PoisonDueCandle = null;

            foreach (var (cause, from) in threats)
            {
                var amulet = p.Hand.FirstOrDefault(i => i.Type == ItemType.Amulet);
                if (amulet is not null)
                {
                    // amulet and weapon leave the game together
                    p.Hand.Remove(amulet);
                    var weapon = p.Hand.FirstOrDefault(i => i.Type == (cause == DeathCause.Dagger ? ItemType.Dagger : ItemType.Poison));
                    if (weapon is not null) p.Hand.Remove(weapon);
                    broken++;
                    Note(m, p.Seat, "Your amulet cracked in your hand. Something meant to kill you tonight.");
                    GiftsAi.OnOwnAmuletBroke(m, p, cause);
                    continue;
                }

                p.Alive = false;
                p.DiedCandle = m.Candle;
                p.DeathCause = cause;
                died.Add(p.Seat);
                m.Deaths++;

                if (p.Role == Role.Assassin && from is { } killer && m.Players[killer] is { Role: Role.Guest, Alive: true } slayer)
                {
                    slayer.Gold += GiftsRules.AssassinSlayerBonus;
                    m.SlayerSeat = killer;
                }
                break;
            }
        }

        foreach (var seat in died)
        {
            var p = m.Players[seat];
            Event(m, "death", $"{p.Name}'s candle goes out. {(p.Role == Role.Assassin ? "The Assassin" : "A Guest")}. In their hand: {HandText(p)}.", seat);
            if (p.Guest?.DeathLine is { Length: > 0 } line) Event(m, "line", line, seat);
            p.Gold = 0; // the host keeps a dead guest's purse
        }
        if (m.SlayerSeat is { } s && died.Any(d => m.Players[d].Role == Role.Assassin))
            Event(m, "host", $"The host pays {m.Players[s].Name} {GiftsRules.AssassinSlayerBonus} gold for the Assassin's death.", s);

        for (var i = 0; i < broken; i++) Event(m, "amulet", "An amulet broke tonight.");
        if (died.Count == 0 && broken == 0) Event(m, "dawn", "Dawn. Every candle still burns.");

        var deltas = new Dictionary<int, int>();
        foreach (var p in m.Players)
        {
            p.LastDelta = p.Gold - p.PublicGold;
            p.PublicGold = p.Gold;
            if (p.LastDelta != 0) deltas[p.Seat] = p.LastDelta;
        }
        var richest = m.Living.OrderByDescending(p => p.Gold).FirstOrDefault();
        if (richest is not null) Event(m, "purse", $"The heaviest purse belongs to {(richest.IsHuman ? "you" : richest.Name)}.", richest.Seat);
        Event(m, "quota", $"Dead: {m.Deaths} of the {m.Quota} the Assassin needs.");

        m.LastDawn = new DawnReport(m.Candle, died, broken, richest?.Seat, deltas);
        GiftsAi.OnDawn(m, died);

        if (!m.Players.Any(p => p is { Role: Role.Assassin, Alive: true })) Finish(m, Winner.Guests);
        else if (m.Deaths >= m.Quota) Finish(m, Winner.Assassin);
        else if (m.Candle >= GiftsRules.Candles)
        {
            m.Phase = GiftsPhase.Pointing;
            Event(m, "host", "The sixth candle gutters out. Name the Assassin — quietly, to me.");
        }
        else m.Phase = GiftsPhase.Dawn;
    }

    public static void DiscardCards(GiftsMatch m, int seat, IReadOnlyList<int> itemIds)
    {
        Require(m, GiftsPhase.Discard);
        var p = Alive(m, seat);
        var excess = p.Hand.Count - GiftsRules.HandLimit;
        if (itemIds.Distinct().Count() != excess) throw new GiftsRuleException($"Discard exactly {excess} card(s).");
        var cards = itemIds.Select(id => p.Hand.FirstOrDefault(i => i.Id == id) ?? throw new GiftsRuleException("That card is not in your hand.")).ToList();
        var nonDaggers = p.Hand.Count(i => i.Type != ItemType.Dagger);
        if (cards.Any(c => c.Type == ItemType.Dagger) && nonDaggers >= excess)
            throw new GiftsRuleException("Daggers cannot be discarded — only given away.");
        foreach (var card in cards)
        {
            p.Hand.Remove(card);
            if (GiftsRules.Deck.ContainsKey(card.Type)) m.Discard.Add(card);
        }
        Run(m);
    }

    // =====================================================================================
    // the end
    // =====================================================================================

    public static void PointAt(GiftsMatch m, int seat, int target)
    {
        Require(m, GiftsPhase.Pointing);
        var p = Alive(m, seat);
        if (p.Role != Role.Guest) throw new GiftsRuleException("Only guests name the Assassin.");
        Other(m, seat, target);
        p.FinalPoint = target;
        Run(m);
    }

    private static void Finish(GiftsMatch m, Winner winner)
    {
        m.Phase = GiftsPhase.Finished;
        m.Winner = winner;

        foreach (var p in m.Players)
            Event(m, "reveal", $"{p.Name} was {(p.Role == Role.Assassin ? "the Assassin" : "a Guest")}.", p.Seat);

        if (winner == Winner.Guests)
        {
            m.Scores = m.Living.Where(p => p.Role == Role.Guest).Select(p =>
            {
                var right = p.FinalPoint is { } t && m.Players[t].Role == Role.Assassin;
                var penalty = p.Count(ItemType.Dagger) * GiftsRules.DaggerPenalty;
                return new FinalScore(p.Seat, p.Gold, right, penalty, p.Gold + (right ? GiftsRules.PointingBonus : 0) - penalty);
            }).OrderByDescending(s => s.Score).ToList();

            var best = m.Scores.Count > 0 ? m.Scores[0].Score : 0;
            m.WinnerSeats = m.Scores.Where(s => s.Score == best).Select(s => s.Seat).ToList();
            Event(m, "end", m.Deaths == 0 && m.Candle >= GiftsRules.Candles
                ? "Every candle burned to the end. The guests win."
                : "The guests win.");
            if (m.WinnerSeats.Count > 0)
                Event(m, "end", $"Guest of honour: {string.Join(", ", m.WinnerSeats.Select(s => m.Players[s].Name))}.");
        }
        else
        {
            m.WinnerSeats = m.Players.Where(p => p.Role == Role.Assassin).Select(p => p.Seat).ToList();
            Event(m, "end", "Enough candles have gone out. The Assassin wins.");
        }
    }

    // =====================================================================================
    // helpers
    // =====================================================================================

    internal static Item NewItem(GiftsMatch m, ItemType type) => new(m.NextItemId++, type);

    private static Item DrawCard(GiftsMatch m)
    {
        if (m.Deck.Count == 0)
        {
            m.Deck.AddRange(m.Discard);
            m.Discard.Clear();
            m.Rng.Shuffle(m.Deck);
            if (m.Deck.Count == 0) return NewItem(m, ItemType.Bread);
        }
        var card = m.Deck[^1];
        m.Deck.RemoveAt(m.Deck.Count - 1);
        return card;
    }

    internal static void DiscardFromHand(GiftsMatch m, GiftsPlayer p, Item card)
    {
        p.Hand.Remove(card);
        if (GiftsRules.Deck.ContainsKey(card.Type)) m.Discard.Add(card);
    }

    internal static Offer AddOffer(GiftsMatch m, GiftsPlayer p, OfferKind kind, int price, string text, bool emergency = false)
    {
        var o = new Offer { Id = m.NextOfferId++, Seat = p.Seat, Candle = m.Candle, Kind = kind, Price = price, Text = text, Emergency = emergency };
        m.Offers.Add(o);
        return o;
    }

    internal static void Event(GiftsMatch m, string kind, string text, int? seat = null, int? target = null) =>
        m.Events.Add(new GiftsEvent(m.Candle, kind, text, seat, target));

    internal static void Note(GiftsMatch m, int seat, string text) => m.Notes.Add(new PrivateNote(m.Candle, seat, text));

    internal static string HandText(GiftsPlayer p) =>
        p.Hand.Count == 0 ? "nothing" : string.Join(", ", p.Hand.GroupBy(i => i.Type).Select(g => g.Count() > 1 ? $"{g.Count()}× {GiftsRules.Name(g.Key)}" : GiftsRules.Name(g.Key)));

    internal static string Article(ItemType t) => t switch
    {
        ItemType.Amulet => "an Amulet",
        ItemType.Bread => "bread",
        ItemType.Wine => "wine",
        ItemType.Poison => "poison",
        ItemType.ClosedDoor => "a Closed Door",
        _ => "a " + GiftsRules.Name(t),
    };

    private static Gift GiftById(GiftsMatch m, int id) => m.Gifts.First(g => g.Id == id);

    private static void Require(GiftsMatch m, GiftsPhase phase)
    {
        if (m.Phase != phase) throw new GiftsRuleException($"That cannot be done now (it is the {m.Phase} phase).");
    }

    private static GiftsPlayer Alive(GiftsMatch m, int seat)
    {
        if (seat < 0 || seat >= m.Players.Count) throw new GiftsRuleException("No such seat.");
        var p = m.Players[seat];
        return p.Alive ? p : throw new GiftsRuleException("The dead cannot do that.");
    }

    private static GiftsPlayer Other(GiftsMatch m, int seat, int? target)
    {
        if (target is not { } t || t < 0 || t >= m.Players.Count || t == seat) throw new GiftsRuleException("Choose another player.");
        var p = m.Players[t];
        return p.Alive ? p : throw new GiftsRuleException($"{p.Name} is dead.");
    }
}
