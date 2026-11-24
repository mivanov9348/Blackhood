namespace Blackhood.Engine;

/// <summary>
/// What the engine needs to know about a participant at the table. Built from the
/// database character by the API; the engine itself knows nothing about storage.
/// Skills, drives and tells join this record when the AI arrives.
/// </summary>
public sealed record TableGuest(
    string Code,
    string Name,
    string Title,
    string Archetype,
    int PortraitHue,
    bool HasPortrait,
    string FirstLine,
    string? FirstLineDirection,
    string HostIntroduction,
    string MetAgainLine,
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
