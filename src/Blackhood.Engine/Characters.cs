namespace Blackhood.Engine;

/// <summary>Skills and drives (1–5) the NPC AI reads. Mirrors the character sheet.</summary>
public sealed record Traits(
    int Insight = 3,
    int Calculation = 3,
    int Cunning = 3,
    int Charisma = 3,
    int Composure = 3,
    int Greed = 3,
    int Boldness = 3,
    int Loyalty = 3,
    int Revenge = 3,
    int Patience = 3,
    int Ambition = 3,
    int Pride = 3,
    int FearOfLoss = 3)
{
    public static readonly Traits Average = new();
}

/// <summary>Internal states a tell can betray (same names as the database enum).</summary>
public enum TellState
{
    Habit,
    Lying,
    Calculating,
    Confident,
    Afraid,
    Tilted,
    PlanningBetrayal,
}

public sealed record Tell(TellState State, string Description, bool Fakeable);

/// <summary>
/// What the engine needs to know about a participant at the table. Built from the
/// database character by the API; the engine itself knows nothing about storage.
/// </summary>
public sealed record TableGuest(
    string Code,
    string Name,
    string Title,
    int PortraitHue,
    bool HasPortrait,
    Traits Traits,
    IReadOnlyList<Tell> Tells,
    string FirstLine,
    string? FirstLineDirection,
    string HostIntroduction,
    string MetAgainLine,
    string DeathLine,
    /// <summary>One visible gesture shown when they sit down — the first lesson in reading tells.</summary>
    string IntroGesture);

/// <summary>The host running the table, with the lines used by the arrival scene.</summary>
public sealed record TableHost(
    string Code,
    string Name,
    string Title,
    int PortraitHue,
    bool HasPortrait,
    string Greeting,
    string MetAgainGreeting,
    string ReplyWho,
    string ReplyBegin,
    string ReplySilent,
    string RoleDeal);
