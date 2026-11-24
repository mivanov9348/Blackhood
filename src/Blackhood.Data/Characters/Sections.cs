namespace Blackhood.Data.Characters;

// Each class below is one section of the character sheet ("NPC CHARACTER SHEET").
// They are EF Core complex types, and the same classes are used in the JSON content
// files and by the admin editor, so there is a single shape everywhere.

/// <summary>Skills (1–5) — how WELL the character does something. Budgeted by rarity.</summary>
public sealed class CharacterSkills
{
    public int Insight { get; set; } = 3;      // Проницателност — reads people
    public int Calculation { get; set; } = 3;  // Пресметливост — reads numbers
    public int Cunning { get; set; } = 3;      // Хитрост — lies well
    public int Charisma { get; set; } = 3;     // Влияние — persuades
    public int Composure { get; set; } = 3;    // Самообладание — doesn't tilt

    [System.Text.Json.Serialization.JsonIgnore]
    public int Sum => Insight + Calculation + Cunning + Charisma + Composure;
}

/// <summary>Drives (1–5) — what the character WANTS. No budget; extremes give personality.</summary>
public sealed class CharacterDrives
{
    public int Greed { get; set; } = 3;        // Алчност
    public int Boldness { get; set; } = 3;     // Дързост (merges "Risk Tolerance")
    public int Loyalty { get; set; } = 3;      // Лоялност
    public int Revenge { get; set; } = 3;      // Отмъстителност
    public int Patience { get; set; } = 3;     // Търпение
    public int Ambition { get; set; } = 3;     // Амбиция
    public int Pride { get; set; } = 3;        // Гордост
    public int FearOfLoss { get; set; } = 3;   // Страх от загуба
}

public sealed class CharacterOrigin
{
    public string Birthplace { get; set; } = "";
    public string SocialStatus { get; set; } = "";
    public string Profession { get; set; } = "";
}

public sealed class CharacterAppearance
{
    public string Face { get; set; } = "";
    public string HairAndEyes { get; set; } = "";
    public string Clothing { get; set; } = "";
    public string Build { get; set; } = "";
    public string Gestures { get; set; } = "";
    public string Voice { get; set; } = "";
    public string FirstImpression { get; set; } = "";
    public string ArtistNotes { get; set; } = "";
    /// <summary>Hue (0–360) for the placeholder portrait until real art exists.</summary>
    public int PortraitHue { get; set; } = 30;
}

public sealed class CharacterBackstory
{
    public string Biography { get; set; } = "";
    public string WhyHere { get; set; } = "";
    public string Secret { get; set; } = "";
}

public sealed class CharacterPsychology
{
    public string CoreTrait { get; set; } = "";
    public string SecondTrait { get; set; } = "";
    public string Weakness { get; set; } = "";
    public string Fear { get; set; } = "";
    public string Desire { get; set; } = "";
    public string Angers { get; set; } = "";
    public string Regret { get; set; } = "";
    public string MoralLimits { get; set; } = "";
    public string NeverWould { get; set; } = "";
    public string WouldForReward { get; set; } = "";
}

public sealed class CharacterBehavior
{
    public string Style { get; set; } = "";
    public string Stakes { get; set; } = "";
    public string Deception { get; set; } = "";
    public string Debts { get; set; } = "";
    public string Alliances { get; set; } = "";
    public string WhenAttacks { get; set; } = "";
    public string WhenRetreats { get; set; } = "";
    public string OnLoss { get; set; } = "";
    public string OnWin { get; set; } = "";
    public string StrategyChange { get; set; } = "";
}

public sealed class CharacterMotives
{
    public string PublicGoal { get; set; } = "";
    public string HiddenGoal { get; set; } = "";
    public string PersonalReason { get; set; } = "";
    public string BetrayalTrigger { get; set; } = "";
    public string HelpTrigger { get; set; } = "";
}

public sealed class CharacterSpeech
{
    public string Register { get; set; } = "";
    public string FavoriteWords { get; set; } = "";
    public string AvoidedTopics { get; set; } = "";
    public string HowLies { get; set; } = "";
    public string AngerAndFear { get; set; } = "";
}

/// <summary>Things the numbers can't do (concept v0.4, "Авторски правила") + the fantasy twist.</summary>
public sealed class CharacterMechanics
{
    /// <summary>„Странното“ — one unexplained, slightly otherworldly thing about the character.</summary>
    public string Strangeness { get; set; } = "";
    /// <summary>Principle / veto — an action they never take, whatever the formula says.</summary>
    public string Principle { get; set; } = "";
    /// <summary>Temptation — a trigger that makes them act irrationally.</summary>
    public string Temptation { get; set; } = "";
    /// <summary>Special ability — breaks one rule, once per match or passively.</summary>
    public string Ability { get; set; } = "";
    /// <summary>Attitude toward the host: fear, debt, service, hatred, ignorance.</summary>
    public string HostRelation { get; set; } = "";
}

public sealed class CharacterAuthorNotes
{
    public string Distinctive { get; set; } = "";
    public string Immutable { get; set; } = "";
    public string Tunable { get; set; } = "";
}

/// <summary>Host-only section: how the host runs the table, plus the 5 host attributes from concept v0.4.</summary>
public sealed class HostProfile
{
    public string Presentation { get; set; } = "";
    public string TrueNature { get; set; } = "";
    public string WantsFromGuests { get; set; } = "";
    public string NeverReveals { get; set; } = "";
    public string RulesAttitude { get; set; } = "";
    public string Punishment { get; set; } = "";
    public string Temptations { get; set; } = "";
    public string Price { get; set; } = "";
    public string Amused { get; set; } = "";
    public string Angered { get; set; } = "";
    public string WouldBreakRules { get; set; } = "";
    public string HostingStyle { get; set; } = "";
    public string SampleOffers { get; set; } = "";
    public string TrueStory { get; set; } = "";
    /// <summary>The one rule this host changes (concept v0.4).</summary>
    public string ChangedRule { get; set; } = "";

    public int Generosity { get; set; } = 3;     // Щедрост
    public int Cruelty { get; set; } = 3;        // Жестокост
    public int Verbosity { get; set; } = 3;      // Словоохотливост
    public int Capriciousness { get; set; } = 3; // Капризност
    public int Partiality { get; set; } = 3;     // Пристрастност: 1 = към водещия, 5 = към изоставащия
}
