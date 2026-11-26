using System.Text.RegularExpressions;

namespace Blackhood.Data.Characters;

public sealed record ValidationResult(IReadOnlyList<string> Errors, IReadOnlyList<string> Warnings)
{
    public bool IsValid => Errors.Count == 0;
}

/// <summary>
/// Errors block saving. Warnings are the design checks from the concept ("checks before a
/// character enters the game") — shown in the editor, but the author decides.
/// </summary>
public static partial class CharacterValidator
{
    /// <summary>Skill budget per rarity (Common 15 / Uncommon 17 / Rare 18). Unique = no budget.</summary>
    public static int? SkillBudget(Rarity rarity) => rarity switch
    {
        Rarity.Common => 15,
        Rarity.Uncommon => 17,
        Rarity.Rare => 18,
        _ => null,
    };

    [GeneratedRegex("^[PH][0-9]{2,3}$")]
    private static partial Regex CodePattern();

    public static ValidationResult Validate(CharacterDocument d)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (!CodePattern().IsMatch(d.Code.Trim().ToUpperInvariant()))
            errors.Add("The code must be P or H followed by 2–3 digits (e.g. P14, H06).");
        else if (d.Code.Trim().ToUpperInvariant()[0] != (d.Kind == CharacterKind.Host ? 'H' : 'P'))
            errors.Add("Participants use P.. codes, hosts use H.. codes.");
        if (string.IsNullOrWhiteSpace(d.Name)) errors.Add("The name is required.");
        if (string.IsNullOrWhiteSpace(d.Title)) errors.Add("The title (shown on the nameplate) is required.");

        var skills = new[] { d.Skills.Insight, d.Skills.Calculation, d.Skills.Cunning, d.Skills.Charisma, d.Skills.Composure };
        var drives = new[] { d.Drives.Greed, d.Drives.Boldness, d.Drives.Loyalty, d.Drives.Revenge, d.Drives.Patience, d.Drives.Ambition, d.Drives.Pride, d.Drives.FearOfLoss };
        if (skills.Concat(drives).Any(v => v is < 1 or > 5))
            errors.Add("Skills and drives must be between 1 and 5.");
        if (d.Host is { } h && new[] { h.Generosity, h.Cruelty, h.Verbosity, h.Capriciousness, h.Partiality }.Any(v => v is < 1 or > 5))
            errors.Add("Host attributes must be between 1 and 5.");
        if (d.Appearance.PortraitHue is < 0 or > 360)
            errors.Add("The portrait hue must be 0–360.");

        if (d.Kind == CharacterKind.Participant)
        {
            if (SkillBudget(d.Rarity) is { } budget && skills.Sum() != budget)
                warnings.Add($"Skills add up to {skills.Sum()}; the {d.Rarity} budget is {budget}.");
            if (!skills.Any(v => v <= 2))
                warnings.Add("No skill at 2 or below — every character needs a weakness.");
            if (d.Rarity == Rarity.Common && skills.Count(v => v == 5) > 1)
                warnings.Add("A Common character has more than one skill at 5.");
            if (drives.Count(v => v is 1 or 5) < 2)
                warnings.Add("Fewer than two drives are extreme (1 or 5) — the character has no personality.");
            if (d.Tells.Count == 0)
                warnings.Add("No tells.");
            else if (d.Rarity != Rarity.Rare && !d.Tells.Any(t => t.VisibleWithoutDossier))
                warnings.Add("At least one tell should be visible without the dossier.");
            if (!d.Tells.Any(t => t.State == TellState.Lying))
                warnings.Add("No 'Lying' tell — in Gifts the table can never catch this character lying.");
            if (!d.Lines.Any(l => l.Situation == LineSituation.FirstMeeting))
                warnings.Add("Missing a first-meeting line (needed for the arrival).");
            if (!d.Lines.Any(l => l.Situation == LineSituation.HostIntroduction))
                warnings.Add("Missing the line the host introduces them with.");
            if (!d.Lines.Any(l => l.Situation == LineSituation.Death))
                warnings.Add("Missing a death line.");
            if (string.IsNullOrWhiteSpace(d.Mechanics.Strangeness))
                warnings.Add("'The Strange' is empty.");
        }
        else
        {
            foreach (var s in new[] { LineSituation.FirstMeeting, LineSituation.IntroReplyWho, LineSituation.IntroReplyBegin, LineSituation.IntroReplySilent, LineSituation.RoleDeal })
                if (!d.Lines.Any(l => l.Situation == s))
                    warnings.Add($"The host has no {s} line (needed for the arrival).");
        }

        return new ValidationResult(errors, warnings);
    }
}
