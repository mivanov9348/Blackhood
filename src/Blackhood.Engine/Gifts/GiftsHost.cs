namespace Blackhood.Engine.Gifts;

/// <summary>
/// The host: neutral, greedy, never lies, never tells everything. Offers are built only
/// from what the player already knows about their own situation plus public facts — the
/// host never sells or hints at roles.
/// </summary>
public static class GiftsHost
{
    public static int AmuletPrice(GiftsMatch m, GiftsPlayer p) =>
        Math.Min(16, 6 + 2 * m.Deaths + (p.ReceivedDaggerLastNight ? 2 : 0) + (m.Candle >= 5 ? 2 : 0));

    public static void MakeOffers(GiftsMatch m, GiftsPlayer p)
    {
        var candidates = new List<(int Priority, OfferKind Kind, int Price, string Text)>();
        var rng = m.Rng;
        var amulets = m.ChestStock[ItemType.Amulet] > 0 && !p.Has(ItemType.Amulet);

        if (p.PoisonDueCandle == m.Candle && amulets)
        {
            var price = AmuletPrice(m, p) + 3;
            candidates.Add((0, OfferKind.Amulet, price, $"There is poison in your blood, my friend. I can save you — {price} gold. Or not."));
        }
        else if (amulets && (p.ReceivedDaggerLastNight || m.Deaths > 0 || m.Candle >= 3 || rng.Chance(0.35)))
        {
            var price = AmuletPrice(m, p);
            var text = m.Deaths > 0
                ? $"Candles are going out, my friend. An amulet — {price} gold."
                : p.ReceivedDaggerLastNight
                    ? $"Last night someone handed you something sharp. An amulet — {price} gold."
                    : $"An amulet, my friend. {price} gold. Something tells me you will need it.";
            candidates.Add((1, OfferKind.Amulet, price, text));
        }

        var richest = m.Living.Max(x => x.Gold);
        if (m.ChestStock[ItemType.Mirror] > 0 && !p.Has(ItemType.Mirror) &&
            ((p.Gold >= richest && rng.Chance(0.6)) || (p.ReceivedDaggerLastNight && rng.Chance(0.3)) || rng.Chance(0.12)))
        {
            var price = 8 + m.Candle / 2;
            candidates.Add((2, OfferKind.Mirror, price, p.Gold >= richest
                ? $"Everyone is watching your purse. A mirror, perhaps? {price} gold."
                : $"A mirror shows a man his own intentions. {price} gold."));
        }

        if (m.ChestStock[ItemType.Poison] > 0 && m.Candle >= 2 && rng.Chance(0.2 + (p.ReceivedDaggerLastNight ? 0.3 : 0)))
        {
            var price = 6 + m.Deaths;
            candidates.Add((3, OfferKind.Poison, price, p.ReceivedDaggerLastNight
                ? $"Someone wished you harm last night. A little vial, to answer them? {price} gold."
                : $"A vial, for the nervous. {price} gold. Remember — every candle that goes out is counted."));
        }

        if (!p.Has(ItemType.ClosedDoor) && rng.Chance(0.3))
            candidates.Add((4, OfferKind.Door, 3, "A door that closes, for 3 gold. Discretion is cheap tonight."));

        foreach (var c in candidates.OrderBy(c => c.Priority).Take(2))
            GiftsGame.AddOffer(m, p, c.Kind, c.Price, c.Text);
    }

    public static void EmergencyOffer(GiftsMatch m, GiftsPlayer p)
    {
        if (m.ChestStock[ItemType.Amulet] <= 0) return;
        var price = Math.Min(18, 9 + 2 * m.Deaths + m.Candle / 2);
        var text = p.ReceivedPoisonTonight
            ? $"That gift tasted bitter, didn't it? An amulet — {price} gold. Decide."
            : $"You just received something sharp. An amulet — {price} gold. Decide.";
        GiftsGame.AddOffer(m, p, OfferKind.Amulet, price, text, emergency: true);
    }

    /// <summary>One true fact about a player's hand. Never their role.</summary>
    public static string Rumor(GiftsMatch m, GiftsPlayer t)
    {
        var facts = new List<string>();
        var daggers = t.Count(ItemType.Dagger);
        if (daggers > 0) facts.Add(daggers == 1 ? $"{t.Name} holds a dagger." : $"{t.Name} holds {daggers} daggers.");
        if (t.Has(ItemType.Amulet)) facts.Add($"{t.Name} keeps an amulet close.");
        if (t.Has(ItemType.Mirror)) facts.Add($"{t.Name} hides a mirror.");
        if (t.Has(ItemType.Poison)) facts.Add($"{t.Name} carries a little vial.");
        if (t.Has(ItemType.ClosedDoor)) facts.Add($"{t.Name} can close a door.");
        if (facts.Count == 0) facts.Add($"{t.Name} holds nothing that can kill, save or shut a door.");
        return m.Rng.Pick(facts);
    }
}
