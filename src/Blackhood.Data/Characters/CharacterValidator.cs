using System.Text.RegularExpressions;

namespace Blackhood.Data.Characters;

public sealed record ValidationResult(IReadOnlyList<string> Errors, IReadOnlyList<string> Warnings)
{
    public bool IsValid => Errors.Count == 0;
}

/// <summary>
/// Errors block saving. Warnings are the design checks from concept v0.4 ("Проверки преди
/// персонажът да влезе в играта") — shown in the editor, but the author decides.
/// </summary>
public static partial class CharacterValidator
{
    /// <summary>Skill budget per rarity (concept: Обикновен 15 / Опасен 17 / Елитен 18). Unique = no budget.</summary>
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
            errors.Add("Кодът трябва да е P или H + 2–3 цифри (напр. P14, H06).");
        else if (d.Code.Trim().ToUpperInvariant()[0] != (d.Kind == CharacterKind.Host ? 'H' : 'P'))
            errors.Add("Участниците са с код P.., домакините — с H..");
        if (string.IsNullOrWhiteSpace(d.Name)) errors.Add("Името е задължително.");
        if (string.IsNullOrWhiteSpace(d.Title)) errors.Add("Титлата (прякорът на табелката) е задължителна.");

        var skills = new[] { d.Skills.Insight, d.Skills.Calculation, d.Skills.Cunning, d.Skills.Charisma, d.Skills.Composure };
        var drives = new[] { d.Drives.Greed, d.Drives.Boldness, d.Drives.Loyalty, d.Drives.Revenge, d.Drives.Patience, d.Drives.Ambition, d.Drives.Pride, d.Drives.FearOfLoss };
        if (skills.Concat(drives).Any(v => v is < 1 or > 5))
            errors.Add("Уменията и подбудите са между 1 и 5.");
        if (d.Host is { } h && new[] { h.Generosity, h.Cruelty, h.Verbosity, h.Capriciousness, h.Partiality }.Any(v => v is < 1 or > 5))
            errors.Add("Атрибутите на домакина са между 1 и 5.");
        if (d.Appearance.PortraitHue is < 0 or > 360)
            errors.Add("Оттенъкът на портрета е 0–360.");

        if (d.Kind == CharacterKind.Participant)
        {
            if (SkillBudget(d.Rarity) is { } budget && skills.Sum() != budget)
                warnings.Add($"Сумата на уменията е {skills.Sum()}, а бюджетът за {d.Rarity} е {budget}.");
            if (!skills.Any(v => v <= 2))
                warnings.Add("Няма умение ≤ 2 — всеки персонаж трябва да има слабост.");
            if (d.Rarity == Rarity.Common && skills.Count(v => v == 5) > 1)
                warnings.Add("Common персонаж има повече от едно умение на 5.");
            if (drives.Count(v => v is 1 or 5) < 2)
                warnings.Add("По-малко от две подбуди са крайни (1 или 5) — персонажът няма личност.");
            if (d.Tells.Count == 0)
                warnings.Add("Няма издайнически знаци.");
            else if (d.Rarity != Rarity.Rare && !d.Tells.Any(t => t.VisibleWithoutDossier))
                warnings.Add("Поне един знак трябва да се вижда без досие.");
            if (!d.Lines.Any(l => l.Situation == LineSituation.FirstMeeting))
                warnings.Add("Липсва реплика за първа среща (нужна за въведението).");
            if (!d.Lines.Any(l => l.Situation == LineSituation.HostIntroduction))
                warnings.Add("Липсва реплика, с която домакинът го представя.");
            if (string.IsNullOrWhiteSpace(d.Mechanics.Strangeness))
                warnings.Add("„Странното“ не е попълнено.");
        }
        else
        {
            foreach (var s in new[] { LineSituation.FirstMeeting, LineSituation.IntroReplyWho, LineSituation.IntroReplyBegin, LineSituation.IntroReplySilent, LineSituation.RoleDeal })
                if (!d.Lines.Any(l => l.Situation == s))
                    warnings.Add($"Домакинът няма реплика за {s} (нужна за въведението).");
        }

        return new ValidationResult(errors, warnings);
    }
}
