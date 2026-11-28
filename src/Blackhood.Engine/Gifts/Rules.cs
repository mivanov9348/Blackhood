namespace Blackhood.Engine.Gifts;

public enum ItemType
{
    // the deck
    Dagger,
    Wine,
    ClosedDoor,
    Monocle,
    Bread,
    // the host's chest — only through offers
    Amulet,
    Poison,
    Mirror,
}

/// <summary>The one word announced with a gift. Dagger and Poison can never be declared.</summary>
public enum Declaration
{
    Purse,
    Bread,
    Wine,
    Monocle,
    ClosedDoor,
    Amulet,
    Mirror,
}

public enum Role
{
    Guest,
    Assassin,
}

public sealed record Item(int Id, ItemType Type);

/// <summary>All the numbers of "Gifts" in one place — tune here, the simulator tells you what changed.</summary>
public static class GiftsRules
{
    public const int Candles = 6;
    public const int StartingGold = 10;
    public const int IncomePerCandle = 2;
    public const int StartingHand = 3;
    public const int AssassinExtraDaggers = 2;
    public const int HandLimit = 5;
    /// <summary>Daggers in hand at the end cost this much gold each.</summary>
    public const int DaggerPenalty = 2;
    /// <summary>Correct guess at the final pointing.</summary>
    public const int PointingBonus = 5;
    /// <summary>The guest who kills the Assassin (mirror or poison) is paid by the host.</summary>
    public const int AssassinSlayerBonus = 5;
    /// <summary>The host sells only this many emergency amulets (the "last whisper") per evening.</summary>
    public const int EmergencyAmuletsPerEvening = 1;
    /// <summary>The first candle is for getting acquainted: the Assassin's daggers are harmless.</summary>
    public const int FirstLethalCandle = 2;

    public static readonly IReadOnlyDictionary<ItemType, int> Deck = new Dictionary<ItemType, int>
    {
        [ItemType.Dagger] = 8,
        [ItemType.Wine] = 6,
        [ItemType.ClosedDoor] = 5,
        [ItemType.Monocle] = 4,
        [ItemType.Bread] = 7,
    };

    public static readonly IReadOnlyDictionary<ItemType, int> Chest = new Dictionary<ItemType, int>
    {
        [ItemType.Amulet] = 4,
        [ItemType.Poison] = 3,
        [ItemType.Mirror] = 2,
    };

    public static int Assassins(int players) => players >= 7 ? 2 : 1;
    public static int Quota(int players) => players >= 7 ? 3 : 2;

    // ---- the host's fixed price list ("bribes") ----
    public const int PriceDoor = 4;
    public const int PriceRumor = 3;
    public const int PriceLearnSender = 4;
    public const int PriceDiversion = 5;
    public const int PricePeek = 4;

    public static bool IsDeclarable(ItemType type) => type is not (ItemType.Dagger or ItemType.Poison);

    public static Declaration? HonestDeclaration(ItemType type) => type switch
    {
        ItemType.Bread => Declaration.Bread,
        ItemType.Wine => Declaration.Wine,
        ItemType.Monocle => Declaration.Monocle,
        ItemType.ClosedDoor => Declaration.ClosedDoor,
        ItemType.Amulet => Declaration.Amulet,
        ItemType.Mirror => Declaration.Mirror,
        _ => null,
    };

    public static string Name(ItemType t) => t switch
    {
        ItemType.ClosedDoor => "Closed Door",
        _ => t.ToString(),
    };

    public static string Name(Declaration d) => d switch
    {
        Declaration.ClosedDoor => "a Closed Door",
        Declaration.Purse => "a Purse",
        Declaration.Bread => "Bread",
        Declaration.Wine => "Wine",
        Declaration.Monocle => "a Monocle",
        Declaration.Amulet => "an Amulet",
        Declaration.Mirror => "a Mirror",
        _ => d.ToString(),
    };
}
