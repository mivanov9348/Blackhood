namespace Blackhood.Engine;

public enum CardType
{
    Gold,
    Dagger,
    Shield,
    Eye,
    Hand,
    Seal,
    Devil,
}

/// <summary>A single card. <see cref="Value"/> is only meaningful for Gold (1–3).</summary>
public sealed record Card(int Id, CardType Type, int Value = 0);

public static class Deck
{
    /// <summary>The standard 40-card deck from concept v0.4.</summary>
    public static List<Card> CreateStandard()
    {
        var cards = new List<Card>(40);
        var id = 1;

        // Gold: 14 cards — 5×1, 5×2, 4×3
        foreach (var (value, count) in new[] { (1, 5), (2, 5), (3, 4) })
            for (var i = 0; i < count; i++)
                cards.Add(new Card(id++, CardType.Gold, value));

        void Add(CardType type, int count)
        {
            for (var i = 0; i < count; i++)
                cards.Add(new Card(id++, type));
        }

        Add(CardType.Dagger, 4);
        Add(CardType.Shield, 5);
        Add(CardType.Eye, 5);
        Add(CardType.Hand, 5);
        Add(CardType.Seal, 3);
        Add(CardType.Devil, 4);

        return cards;
    }

    public static void Shuffle<T>(IList<T> list, Random rng)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
