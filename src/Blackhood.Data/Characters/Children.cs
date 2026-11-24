namespace Blackhood.Data.Characters;

/// <summary>When a line is spoken.</summary>
public enum LineSituation
{
    // from the character sheet
    FirstMeeting,
    Win,
    Loss,
    Accused,
    AllianceOffer,
    Betrayal,
    BigBet,

    // the arrival (intro) scene
    /// <summary>Spoken by the HOST about this participant when they sit down.</summary>
    HostIntroduction,
    /// <summary>Participant: shorter greeting when the player has met them before.</summary>
    MetAgain,
    /// <summary>Host: reply when the player asks "Who are you?".</summary>
    IntroReplyWho,
    /// <summary>Host: reply when the player says "Let's begin."</summary>
    IntroReplyBegin,
    /// <summary>Host: reply when the player stays silent.</summary>
    IntroReplySilent,
    /// <summary>Host: line when sliding the secret role card to the player.</summary>
    RoleDeal,

    // later in the game
    Claim,
    Defend,
    Robbed,
    Tilted,
    Death,
    RoleReveal,
    ToHost,
}

public sealed class CharacterLine
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public LineSituation Situation { get; set; }
    public string Text { get; set; } = "";
    /// <summary>Stage direction shown with the line, e.g. "казано с пълна липса на изненада".</summary>
    public string? Direction { get; set; }
    public int Order { get; set; }
}

/// <summary>Internal state a tell can reveal.</summary>
public enum TellState
{
    /// <summary>A habit with no meaning (yet) — imported from "Жестове и навици".</summary>
    Habit,
    Lying,
    Calculating,
    Confident,
    Afraid,
    Tilted,
    PlanningBetrayal,
}

/// <summary>A visible tell (gesture or line) tied to an internal state.</summary>
public sealed class CharacterTell
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public TellState State { get; set; }
    public string Description { get; set; } = "";
    /// <summary>Characters with high Cunning can show this tell falsely.</summary>
    public bool Fakeable { get; set; }
    /// <summary>Visible from the first meeting (otherwise unlocked through the dossier).</summary>
    public bool VisibleWithoutDossier { get; set; } = true;
    public int Order { get; set; }
}

/// <summary>A special relationship with history (concept v0.4, "Етаж 2").</summary>
public sealed class CharacterRelation
{
    public int Id { get; set; }
    public int FromCharacterId { get; set; }
    public int ToCharacterId { get; set; }
    public Character? To { get; set; }
    /// <summary>-100 … +100</summary>
    public int Value { get; set; }
    public string Story { get; set; } = "";
    public string Effect { get; set; } = "";
}

public enum AssetKind
{
    Portrait,
}

public enum Expression
{
    Neutral,
    Speaking,
    Smug,
    Tilted,
    Afraid,
    Dead,
}

public sealed class CharacterAsset
{
    public int Id { get; set; }
    public int CharacterId { get; set; }
    public AssetKind Kind { get; set; }
    public Expression Expression { get; set; }
    /// <summary>File name inside the asset storage folder.</summary>
    public string FileName { get; set; } = "";
    public string ContentType { get; set; } = "";
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>How many times a player has sat at a table with a character — drives the short intro and, later, the dossier.</summary>
public sealed class PlayerEncounter
{
    public string UserId { get; set; } = "";
    public int CharacterId { get; set; }
    public Character? Character { get; set; }
    public int Count { get; set; }
    public DateTime FirstMetAt { get; set; } = DateTime.UtcNow;
    public DateTime LastMetAt { get; set; } = DateTime.UtcNow;
}
