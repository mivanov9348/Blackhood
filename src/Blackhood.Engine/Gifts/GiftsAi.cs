namespace Blackhood.Engine.Gifts;

/// <summary>
/// NPC decisions for "Gifts". Every choice is a probability shaped by the character's
/// skills and drives (Traits), so two NPCs in the same spot behave differently:
/// the suspicious close doors, the greedy never pay, the vengeful send daggers back,
/// the fearful buy amulets at any price. The AI only uses what that seat could know.
/// </summary>
public static class GiftsAi
{
    // =====================================================================================
    // beliefs
    // =====================================================================================

    public static void InitBeliefs(GiftsMatch m)
    {
        foreach (var p in m.Players)
        {
            p.Suspicion = new double[m.Players.Count];
            if (p.Role == Role.Assassin)
                p.KnownInnocent = m.Players.Where(x => x.Role == Role.Assassin && x != p).Select(x => x.Seat).ToList();
        }
    }

    /// <summary>How strongly <paramref name="p"/> suspects <paramref name="t"/> of being the Assassin.</summary>
    public static double Suspect(GiftsMatch m, GiftsPlayer p, GiftsPlayer t)
    {
        if (t == p || !t.Alive) return 0;
        if (p.KnownKiller == t.Seat) return 100;
        if (p.KnownInnocent.Contains(t.Seat)) return 0.05;
        return 1 + p.Suspicion[t.Seat];
    }

    /// <summary>Re-derives hard facts (innocent / killer) from everything this player has learned.</summary>
    private static void Recompute(GiftsMatch m, GiftsPlayer p)
    {
        if (p.Role == Role.Assassin) return;

        foreach (var id in p.KnownGiftSenders)
        {
            var g = m.Gifts.First(x => x.Id == id);
            if (g.From == p.Seat || g.Card?.Type != ItemType.Dagger) continue;

            // A dagger I accepted, from a sender I know.
            if (g.To == p.Seat && g.Outcome == GiftOutcome.Accepted && g.Candle >= GiftsRules.FirstLethalCandle)
            {
                var daggersThatNight = m.Gifts.Count(x => x.Candle == g.Candle && x.To == p.Seat && x.Outcome == GiftOutcome.Accepted && x.Card?.Type == ItemType.Dagger);
                var dawnPassed = g.Candle < m.Candle || m.Phase is GiftsPhase.Dawn or GiftsPhase.Discard or GiftsPhase.Pointing or GiftsPhase.Finished;
                if (p.AmuletBrokeCandles.Contains(g.Candle) && daggersThatNight == 1) p.KnownKiller = g.From;
                else if (dawnPassed && !p.AmuletBrokeCandles.Contains(g.Candle)) AddInnocent(p, g.From);
            }
        }

        // A victim who received a single gift that night, from a sender I know → that sender killed.
        foreach (var id in p.KnownGiftSenders)
        {
            var g = m.Gifts.First(x => x.Id == id);
            var victim = m.Players[g.To];
            if (victim.Alive || victim.DiedCandle != g.Candle || victim.DeathCause != DeathCause.Dagger) continue;
            var giftsToVictim = m.Gifts.Count(x => x.Candle == g.Candle && x.To == victim.Seat && x.Outcome == GiftOutcome.Accepted);
            if (giftsToVictim == 1 && g.From != p.Seat) p.KnownKiller = g.From;
        }

        foreach (var dead in m.Players.Where(x => !x.Alive && x.Role == Role.Guest)) AddInnocent(p, dead.Seat);
        if (p.KnownKiller is { } k && p.KnownInnocent.Contains(k)) p.KnownInnocent.Remove(k);
    }

    private static void AddInnocent(GiftsPlayer p, int seat)
    {
        if (seat != p.Seat && !p.KnownInnocent.Contains(seat)) p.KnownInnocent.Add(seat);
    }

    private static GiftsPlayer? TopSuspect(GiftsMatch m, GiftsPlayer p)
    {
        var others = m.Living.Where(x => x != p && !(p.Role == Role.Assassin && x.Role == Role.Assassin)).ToList();
        if (others.Count == 0) return null;
        var best = others.Max(x => Suspect(m, p, x));
        var top = others.Where(x => Suspect(m, p, x) >= best - 1e-9).ToList();
        return m.Rng.Pick(top);
    }

    // ---- observations ----

    public static void OnSawHand(GiftsMatch m, GiftsPlayer p, GiftsPlayer t, bool partial)
    {
        if (t.Has(ItemType.Amulet) && !p.SeenAmulet.Contains(t.Seat)) p.SeenAmulet.Add(t.Seat);
        if (!t.Has(ItemType.Amulet)) p.SeenAmulet.Remove(t.Seat);
        p.Suspicion[t.Seat] += t.Count(ItemType.Dagger) * (partial ? 0.15 : 0.3);
    }

    public static void OnGhostPoint(GiftsMatch m, GiftsPlayer ghost, int target)
    {
        foreach (var p in m.Living) p.Suspicion[target] += 0.6;
    }

    public static void OnReceived(GiftsMatch m, GiftsPlayer r, Gift g, bool mirrored) { }

    public static void OnLearnedSender(GiftsMatch m, GiftsPlayer p, Gift g)
    {
        if (!p.KnownGiftSenders.Contains(g.Id)) p.KnownGiftSenders.Add(g.Id);
        if (g.To == p.Seat && g.IsHarmful && g.From != p.Seat) p.Suspicion[g.From] += 1.0;
        Recompute(m, p);
    }

    public static void OnOwnAmuletBroke(GiftsMatch m, GiftsPlayer p, DeathCause cause)
    {
        p.AmuletBrokeCandles.Add(m.Candle);
        Recompute(m, p);
    }

    public static void OnDawn(GiftsMatch m, List<int> died)
    {
        foreach (var p in m.Players)
        {
            Recompute(m, p);
            if (p.Role != Role.Assassin) continue;

            // My lethal daggers that did not kill: their amulet is spent now.
            foreach (var g in m.Tonight.Where(x => x.From == p.Seat && x.Outcome == GiftOutcome.Accepted && x.Card?.Type == ItemType.Dagger))
            {
                var t = m.Players[g.To];
                if (t.Alive && t.Role == Role.Guest && m.Candle >= GiftsRules.FirstLethalCandle && !p.Unprotected.Contains(t.Seat))
                {
                    p.Unprotected.Add(t.Seat);
                    p.SeenAmulet.Remove(t.Seat);
                }
            }
            p.Unprotected.RemoveAll(s => !m.Players[s].Alive || m.Players[s].LastDelta <= -6); // bought a new one?
        }
    }

    // =====================================================================================
    // whisper
    // =====================================================================================

    public static void Whisper(GiftsMatch m, GiftsPlayer p)
    {
        var t = p.Traits;
        var rng = m.Rng;

        // Monocle: look at whoever worries me most (or, as the Assassin, at a target).
        var monocle = p.Hand.FirstOrDefault(i => i.Type == ItemType.Monocle);
        if (monocle is not null && m.Candle >= 2 && rng.Chance(0.6) && TopSuspect(m, p) is { } look)
            GiftsGame.PlayMonocle(m, p.Seat, monocle.Id, look.Seat);

        foreach (var o in m.Offers.Where(o => o.Seat == p.Seat && o.Candle == m.Candle && !o.Emergency && !o.Resolved).ToList())
        {
            var desire = o.Kind switch
            {
                OfferKind.Amulet when p.Has(ItemType.Amulet) => 0,
                OfferKind.Amulet when p.Role == Role.Assassin => 0.06 + t.FearOfLoss * 0.03,
                OfferKind.Amulet => 0.04 + t.FearOfLoss * 0.06 + m.Deaths * 0.12
                                    + (p.ReceivedDaggerLastNight ? 0.25 : 0)
                                    + (p.PoisonDueCandle == m.Candle ? 0.9 : 0)
                                    - (t.Greed - 3) * 0.07,
                OfferKind.Mirror when p.Role == Role.Assassin => 0.04,
                OfferKind.Mirror => 0.06 + (p.KnownKiller is not null ? 0.35 : 0) + t.Boldness * 0.025 + m.Deaths * 0.05,
                OfferKind.Poison when p.Role == Role.Assassin => 0.3 + t.Boldness * 0.04,
                OfferKind.Poison => p.KnownKiller is not null ? 0.9 : (t.Ambition >= 5 && t.Greed >= 4 ? 0.08 : 0.02),
                OfferKind.Door when p.Has(ItemType.ClosedDoor) => 0.05,
                OfferKind.Door => 0.2 + (t.FearOfLoss - 3) * 0.1,
                _ => 0,
            };
            var reserve = t.Greed >= 4 ? 4 : 1;
            var mustHave = o.Kind == OfferKind.Amulet && p.PoisonDueCandle == m.Candle;
            var affordable = p.Gold >= o.Price + (mustHave ? 0 : reserve);
            GiftsGame.AcceptOffer(m, p.Seat, o.Id, affordable && rng.Chance(Math.Clamp(desire, 0, 0.97)));
        }

        // A few fixed-price services, by temperament.
        if (t.Insight >= 4 && p.Gold >= 9 && m.Candle >= 2 && rng.Chance(0.15) && TopSuspect(m, p) is { } peek)
            GiftsGame.BuyService(m, p.Seat, OfferKind.Peek, peek.Seat);
        else if (p.Role == Role.Assassin && p.Gold >= 8 && m.Candle >= 2 && rng.Chance(0.25) && PickVictim(m, p) is { } scout)
            GiftsGame.BuyService(m, p.Seat, OfferKind.Peek, scout.Seat);

        if (t.FearOfLoss >= 4 && !p.Has(ItemType.ClosedDoor) && m.Deaths > 0 && p.Gold >= GiftsRules.PriceDoor + 2 && rng.Chance(0.3))
            GiftsGame.BuyService(m, p.Seat, OfferKind.Door);

        if (t.FearOfLoss >= 5 && p.ReceivedDaggerLastNight && !p.DiversionTonight && p.Gold >= GiftsRules.PriceDiversion + 2 && rng.Chance(0.3))
            GiftsGame.BuyService(m, p.Seat, OfferKind.Diversion);
    }

    // =====================================================================================
    // gifts
    // =====================================================================================

    public static void SendGift(GiftsMatch m, GiftsPlayer p)
    {
        var t = p.Traits;
        var rng = m.Rng;
        var others = m.Living.Where(x => x != p).ToList();
        if (others.Count == 0) return;

        // ---- the Assassin strikes ----
        if (p.Role == Role.Assassin && m.Candle >= GiftsRules.FirstLethalCandle && p.Has(ItemType.Dagger))
        {
            var remaining = m.Quota - m.Deaths;
            var nightsLeft = GiftsRules.Candles - m.Candle + 1;
            var strike = Math.Clamp(0.35 + (double)remaining / nightsLeft * 0.8 + (t.Boldness - 3) * 0.08 - (t.Patience - 3) * 0.05, 0.15, 0.95);
            if (rng.Chance(strike) && PickVictim(m, p) is { } victim)
            {
                Send(m, p, victim, p.Hand.First(i => i.Type == ItemType.Dagger), Lie(m, p));
                return;
            }
        }

        // ---- poison for the one I know (or strongly believe) is the killer ----
        var poison = p.Hand.FirstOrDefault(i => i.Type == ItemType.Poison);
        if (poison is not null)
        {
            GiftsPlayer? target = null;
            if (p.Role == Role.Assassin) target = PickVictim(m, p);
            else if (p.KnownKiller is { } k && m.Players[k].Alive) target = m.Players[k];
            else if (TopSuspect(m, p) is { } s && Suspect(m, p, s) >= 2.5 && t.Boldness >= 3 && m.Quota - m.Deaths > 1) target = s;
            if (target is not null)
            {
                Send(m, p, target, poison, Lie(m, p));
                return;
            }
        }

        // ---- get rid of a dagger (−2 gold at the end) ----
        var dagger = p.Hand.FirstOrDefault(i => i.Type == ItemType.Dagger);
        if (dagger is not null && p.Role == Role.Guest &&
            rng.Chance(0.25 + t.Greed * 0.08 + (p.Hand.Count >= 4 ? 0.2 : 0) - t.Loyalty * 0.03))
        {
            var target = Revenge(m, p) ?? rng.Pick(others);
            Send(m, p, target, dagger, Lie(m, p));
            return;
        }

        // ---- a loyal soul with a spare amulet gives one away ----
        if (t.Loyalty >= 4 && p.Count(ItemType.Amulet) >= 2)
        {
            var poorest = others.OrderBy(x => x.PublicGold).First();
            Send(m, p, poorest, p.Hand.First(i => i.Type == ItemType.Amulet), Declaration.Amulet);
            return;
        }

        // ---- otherwise an honest gift: a card if the hand is crowded, free bread if not ----
        var friend = others.OrderBy(x => Suspect(m, p, x) + rng.NextDouble()).First();
        var spare = p.Hand.FirstOrDefault(i => i.Type == ItemType.Bread)
                    ?? (p.Hand.Count >= GiftsRules.HandLimit ? p.Hand.FirstOrDefault(i => i.Type is ItemType.Wine or ItemType.Monocle) : null);
        if (spare is not null)
            Send(m, p, friend, spare, GiftsRules.HonestDeclaration(spare.Type)!.Value);
        else
            GiftsGame.SendGift(m, p.Seat, GiftKind.FreeBread, friend.Seat, Declaration.Bread);
    }

    private static void Send(GiftsMatch m, GiftsPlayer p, GiftsPlayer to, Item card, Declaration d) =>
        GiftsGame.SendGift(m, p.Seat, GiftKind.Card, to.Seat, d, card.Id);

    /// <summary>What a liar announces: the cunning pick quieter words.</summary>
    private static Declaration Lie(GiftsMatch m, GiftsPlayer p)
    {
        var weights = p.Traits.Cunning >= 4
            ? new double[] { 0.30, 0.45, 0.15, 0.10 }   // Purse, Bread, Wine, Monocle
            : new double[] { 0.45, 0.25, 0.25, 0.05 };
        return m.Rng.Weighted(weights) switch
        {
            0 => Declaration.Purse,
            1 => Declaration.Bread,
            2 => Declaration.Wine,
            _ => Declaration.Monocle,
        };
    }

    /// <summary>The Assassin's choice of victim.</summary>
    private static GiftsPlayer? PickVictim(GiftsMatch m, GiftsPlayer a)
    {
        var targets = m.Living.Where(x => x != a && x.Role == Role.Guest).ToList();
        if (targets.Count == 0) return null;
        var weights = targets.Select(t =>
        {
            var w = 1.0;
            if (a.SeenAmulet.Contains(t.Seat)) w *= 0.15;
            if (t.LastDelta <= -5) w *= 0.4;              // just paid the host — an amulet?
            if (a.Unprotected.Contains(t.Seat)) w *= 3.0;  // spent their amulet on me
            return w;
        }).ToList();
        var i = m.Rng.Weighted(weights);
        return i < 0 ? null : targets[i];
    }

    private static GiftsPlayer? Revenge(GiftsMatch m, GiftsPlayer p)
    {
        if (p.Traits.Revenge < 4) return null;
        var harmer = p.KnownGiftSenders
            .Select(id => m.Gifts.First(g => g.Id == id))
            .Where(g => g.To == p.Seat && g.IsHarmful && m.Players[g.From].Alive && g.From != p.Seat)
            .Select(g => m.Players[g.From])
            .LastOrDefault();
        return harmer;
    }

    // =====================================================================================
    // delivery
    // =====================================================================================

    public static DeliveryResponse Respond(GiftsMatch m, GiftsPlayer r, Gift g)
    {
        var t = r.Traits;
        var s = m.Players[g.From];
        var rng = m.Rng;

        var harmful = g.Card?.Type == ItemType.Poison || (g.Card?.Type == ItemType.Dagger && m.Candle >= GiftsRules.FirstLethalCandle);
        double p;
        if (harmful)
        {
            var detect = Math.Clamp(0.35 + 0.12 * (t.Insight - s.Traits.Cunning), 0.05, 0.9);
            p = rng.Chance(detect) ? 0.75 : 0.15;
        }
        else p = 0.06 + (6 - t.Composure) * 0.02 + (t.FearOfLoss - 3) * 0.03;

        var sameWord = m.Tonight.Count(x => x.Announced && x.Declared == g.Declared);
        p += 0.08 * (sameWord - 1);
        if (m.Candle >= GiftsRules.FirstLethalCandle) p += 0.08 * m.Deaths;
        if (m.Candle < GiftsRules.FirstLethalCandle && t.Calculation >= 3) p *= 0.2; // knows daggers can't kill yet
        if (r.Has(ItemType.Amulet)) p *= 0.4;
        if (g.Declared == Declaration.Purse && t.Greed >= 4) p *= 0.5;
        if (t.Patience >= 4 && m.Candle <= 2) p *= 0.7;

        if (!rng.Chance(Math.Clamp(p, 0, 0.95))) return DeliveryResponse.Accept;
        if (r.Has(ItemType.Mirror) && (p >= 0.5 || !r.Has(ItemType.ClosedDoor))) return DeliveryResponse.Mirror;
        return r.Has(ItemType.ClosedDoor) ? DeliveryResponse.CloseDoor : DeliveryResponse.Accept;
    }

    public static Gift? ChooseWine(GiftsMatch m, GiftsPlayer p)
    {
        var tonight = m.Tonight.Where(g => g.Announced && g.From != p.Seat).ToList();
        if (tonight.Count == 0) return null;
        var mine = tonight.Where(g => g.To == p.Seat && g.Outcome == GiftOutcome.Accepted).ToList();
        return mine.LastOrDefault(g => g.IsHarmful)
               ?? (m.Rng.Chance(0.5) && mine.Count > 0 ? m.Rng.Pick(mine) : m.Rng.Pick(tonight));
    }

    public static bool AcceptEmergency(GiftsMatch m, GiftsPlayer p, Offer o)
    {
        if (p.Gold < o.Price) return false;
        var t = p.Traits;
        if (p.Role == Role.Assassin) return (p.LethalTonight || p.ReceivedPoisonTonight) && m.Rng.Chance(0.85);
        if (!p.ReceivedPoisonTonight && m.Candle < GiftsRules.FirstLethalCandle && t.Calculation >= 3) return false;
        var chance = 0.15 + t.FearOfLoss * 0.09 - (t.Greed - 3) * 0.08 + (m.Deaths > 0 ? 0.1 : 0) + (p.ReceivedPoisonTonight ? 0.3 : 0);
        return m.Rng.Chance(Math.Clamp(chance, 0.05, 0.97));
    }

    // =====================================================================================
    // dawn & the end
    // =====================================================================================

    public static void TrimHand(GiftsMatch m, GiftsPlayer p)
    {
        // Daggers cannot be discarded — only given away.
        var order = new[] { ItemType.Bread, ItemType.Wine, ItemType.Monocle, ItemType.ClosedDoor, ItemType.Poison, ItemType.Mirror, ItemType.Amulet };
        while (p.Hand.Count > GiftsRules.HandLimit)
        {
            var card = order.Select(type => p.Hand.FirstOrDefault(i => i.Type == type)).FirstOrDefault(c => c is not null)
                       ?? p.Hand.First(); // only daggers left — then a dagger may go
            GiftsGame.DiscardFromHand(m, p, card);
        }
    }

    public static int? GhostPoint(GiftsMatch m, GiftsPlayer ghost)
    {
        if (ghost.KnownKiller is { } k && m.Players[k].Alive) return k;
        var living = m.Living.ToList();
        if (living.Count == 0) return null;
        var scores = living.Select(x => Suspect(m, ghost, x)).ToList();
        var mean = scores.Average();
        var best = scores.Max();
        return best >= mean * 1.5 && best > 1.2 ? living[scores.IndexOf(best)].Seat : null;
    }

    public static int FinalPoint(GiftsMatch m, GiftsPlayer p) => TopSuspect(m, p)?.Seat ?? m.Living.First(x => x != p).Seat;

    // =====================================================================================
    // tells — the table sees a gesture, never the reason
    // =====================================================================================

    public static void MaybeShowLyingTell(GiftsMatch m, GiftsPlayer p, Gift g)
    {
        if (p.IsHuman || p.Guest is null) return;
        var tell = p.Guest.Tells.FirstOrDefault(x => x.State is TellState.Lying or TellState.PlanningBetrayal);
        if (tell is null) return;

        var honest = g.Kind switch
        {
            GiftKind.Purse => g.Declared == Declaration.Purse,
            GiftKind.FreeBread => g.Declared == Declaration.Bread,
            _ => GiftsRules.HonestDeclaration(g.Card!.Type) == g.Declared,
        };
        var show = honest
            ? tell.Fakeable && p.Traits.Cunning >= 4 && m.Rng.Chance(0.08)
            : m.Rng.Chance((6 - p.Traits.Composure) * 0.07);
        if (show) GiftsGame.Event(m, "tell", Gesture(p, tell), p.Seat);
    }

    public static void MaybeShowAfraidTell(GiftsMatch m, GiftsPlayer p)
    {
        if (p.IsHuman || p.Guest is null) return;
        var tell = p.Guest.Tells.FirstOrDefault(x => x.State == TellState.Afraid);
        if (tell is not null && m.Rng.Chance((6 - p.Traits.Composure) * 0.08))
            GiftsGame.Event(m, "tell", Gesture(p, tell), p.Seat);
    }

    private static string Gesture(GiftsPlayer p, Tell tell)
    {
        var d = tell.Description.Trim().TrimEnd('.');
        return d.Length == 0 ? p.Name : $"{p.Name} {char.ToLowerInvariant(d[0])}{d[1..]}.";
    }
}
