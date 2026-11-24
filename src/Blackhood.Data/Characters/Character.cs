namespace Blackhood.Data.Characters;

public enum CharacterKind
{
    Participant,
    Host,
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Unique,
}

public enum ContentStatus
{
    Draft,
    Published,
}

/// <summary>
/// A character — table participant or host. Numbers live in columns (the AI and the
/// simulator read them constantly); long authored text lives in JSON columns, one per
/// section of the character sheet, so the sheet can grow without a migration per field.
/// </summary>
public sealed class Character
{
    public int Id { get; set; }

    /// <summary>Stable code from the character sheet: P01…, H01…</summary>
    public string Code { get; set; } = "";
    public CharacterKind Kind { get; set; }
    public string Name { get; set; } = "";
    /// <summary>Short title shown on the nameplate, e.g. "Надменният лорд".</summary>
    public string Title { get; set; } = "";
    /// <summary>Nicknames / what others call them.</summary>
    public string Nickname { get; set; } = "";
    public string Gender { get; set; } = "";
    /// <summary>Free text — hosts have ages like "изглежда на 65".</summary>
    public string Age { get; set; } = "";
    public string Archetype { get; set; } = "";
    public Rarity Rarity { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public List<string> Tags { get; set; } = [];

    public int Version { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // ---- numbers (columns) ----
    public CharacterSkills Skills { get; set; } = new();
    public CharacterDrives Drives { get; set; } = new();

    // ---- authored sections (JSON columns) ----
    public CharacterOrigin Origin { get; set; } = new();
    public CharacterAppearance Appearance { get; set; } = new();
    public CharacterBackstory Backstory { get; set; } = new();
    public CharacterPsychology Psychology { get; set; } = new();
    public CharacterBehavior Behavior { get; set; } = new();
    public CharacterMotives Motives { get; set; } = new();
    public CharacterSpeech Speech { get; set; } = new();
    public CharacterMechanics Mechanics { get; set; } = new();
    public CharacterAuthorNotes AuthorNotes { get; set; } = new();
    /// <summary>Only filled for <see cref="CharacterKind.Host"/>.</summary>
    public HostProfile Host { get; set; } = new();

    // ---- child tables ----
    public List<CharacterLine> Lines { get; set; } = [];
    public List<CharacterTell> Tells { get; set; } = [];
    public List<CharacterRelation> Relations { get; set; } = [];
    public List<CharacterAsset> Assets { get; set; } = [];
}
