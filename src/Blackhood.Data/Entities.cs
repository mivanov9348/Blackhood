using Microsoft.AspNetCore.Identity;

namespace Blackhood.Data;

public sealed class AppUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    /// <summary>The character's purse — every tavern game is played with this gold.</summary>
    public int Gold { get; set; } = AppUser.StartingGold;
    public List<MatchRecord> Matches { get; set; } = [];

    public const int StartingGold = 50;
}

public enum MatchMode
{
    Singleplayer,
    Multiplayer,
}

public static class GameKinds
{
    public const string Gifts = "Gifts";
    /// <summary>Matches from the first prototype (secret roles + market). No longer playable.</summary>
    public const string Legacy = "Legacy";
}

/// <summary>One played (or ongoing) match. The engine state is stored as JSON.</summary>
public sealed class MatchRecord
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = "";
    public AppUser? User { get; set; }
    public MatchMode Mode { get; set; }
    /// <summary>Which tavern game this is (see <see cref="GameKinds"/>).</summary>
    public string Game { get; set; } = GameKinds.Gifts;
    public int Seed { get; set; }
    /// <summary>Gold taken from the player's purse to sit at the table.</summary>
    public int BuyIn { get; set; }
    /// <summary>Gold paid back to the player's purse when the evening ended.</summary>
    public int Payout { get; set; }
    /// <summary>The payout has been added to the player's purse.</summary>
    public bool Settled { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    public string StateJson { get; set; } = "";
}
