namespace Blackhood.Data.Characters;

/// <summary>
/// The portable shape of a character: used for content/characters/*.json AND by the
/// admin editor API. Relations reference other characters by <see cref="Code"/>.
/// </summary>
public sealed class CharacterDocument
{
    public string Code { get; set; } = "";
    public CharacterKind Kind { get; set; }
    public string Name { get; set; } = "";
    public string Title { get; set; } = "";
    public string Nickname { get; set; } = "";
    public string Gender { get; set; } = "";
    public string Age { get; set; } = "";
    public string Archetype { get; set; } = "";
    public Rarity Rarity { get; set; }
    public ContentStatus Status { get; set; } = ContentStatus.Draft;
    public List<string> Tags { get; set; } = [];
    /// <summary>Optimistic concurrency for the editor; ignored in content files.</summary>
    public int Version { get; set; }

    public CharacterSkills Skills { get; set; } = new();
    public CharacterDrives Drives { get; set; } = new();
    public CharacterOrigin Origin { get; set; } = new();
    public CharacterAppearance Appearance { get; set; } = new();
    public CharacterBackstory Backstory { get; set; } = new();
    public CharacterPsychology Psychology { get; set; } = new();
    public CharacterBehavior Behavior { get; set; } = new();
    public CharacterMotives Motives { get; set; } = new();
    public CharacterSpeech Speech { get; set; } = new();
    public CharacterMechanics Mechanics { get; set; } = new();
    public CharacterAuthorNotes AuthorNotes { get; set; } = new();
    public HostProfile? Host { get; set; }

    public List<LineDocument> Lines { get; set; } = [];
    public List<TellDocument> Tells { get; set; } = [];
    public List<RelationDocument> Relations { get; set; } = [];
}

public sealed class LineDocument
{
    public LineSituation Situation { get; set; }
    public string Text { get; set; } = "";
    public string? Direction { get; set; }
}

public sealed class TellDocument
{
    public TellState State { get; set; }
    public string Description { get; set; } = "";
    public bool Fakeable { get; set; }
    public bool VisibleWithoutDossier { get; set; } = true;
}

public sealed class RelationDocument
{
    public string To { get; set; } = "";
    public int Value { get; set; }
    public string Story { get; set; } = "";
    public string Effect { get; set; } = "";
}

public static class CharacterMapping
{
    public static CharacterDocument ToDocument(this Character c, IReadOnlyDictionary<int, string> codesById) => new()
    {
        Code = c.Code,
        Kind = c.Kind,
        Name = c.Name,
        Title = c.Title,
        Nickname = c.Nickname,
        Gender = c.Gender,
        Age = c.Age,
        Archetype = c.Archetype,
        Rarity = c.Rarity,
        Status = c.Status,
        Tags = [.. c.Tags],
        Version = c.Version,
        Skills = c.Skills,
        Drives = c.Drives,
        Origin = c.Origin,
        Appearance = c.Appearance,
        Backstory = c.Backstory,
        Psychology = c.Psychology,
        Behavior = c.Behavior,
        Motives = c.Motives,
        Speech = c.Speech,
        Mechanics = c.Mechanics,
        AuthorNotes = c.AuthorNotes,
        Host = c.Kind == CharacterKind.Host ? c.Host : null,
        Lines = c.Lines.OrderBy(l => l.Situation).ThenBy(l => l.Order)
            .Select(l => new LineDocument { Situation = l.Situation, Text = l.Text, Direction = l.Direction }).ToList(),
        Tells = c.Tells.OrderBy(t => t.Order)
            .Select(t => new TellDocument { State = t.State, Description = t.Description, Fakeable = t.Fakeable, VisibleWithoutDossier = t.VisibleWithoutDossier }).ToList(),
        Relations = c.Relations
            .Where(r => codesById.ContainsKey(r.ToCharacterId))
            .Select(r => new RelationDocument { To = codesById[r.ToCharacterId], Value = r.Value, Story = r.Story, Effect = r.Effect }).ToList(),
    };

    /// <summary>
    /// Copies everything except relations from the document onto the entity.
    /// Relations need the other characters' ids — see <see cref="ApplyRelations"/>.
    /// </summary>
    public static void ApplyDocument(this Character c, CharacterDocument d)
    {
        c.Code = d.Code.Trim().ToUpperInvariant();
        c.Kind = d.Kind;
        c.Name = d.Name.Trim();
        c.Title = d.Title.Trim();
        c.Nickname = d.Nickname.Trim();
        c.Gender = d.Gender.Trim();
        c.Age = d.Age.Trim();
        c.Archetype = d.Archetype.Trim();
        c.Rarity = d.Rarity;
        c.Status = d.Status;
        c.Tags = d.Tags.Select(t => t.Trim().ToLowerInvariant()).Where(t => t.Length > 0).Distinct().ToList();
        c.Skills = d.Skills;
        c.Drives = d.Drives;
        c.Origin = d.Origin;
        c.Appearance = d.Appearance;
        c.Backstory = d.Backstory;
        c.Psychology = d.Psychology;
        c.Behavior = d.Behavior;
        c.Motives = d.Motives;
        c.Speech = d.Speech;
        c.Mechanics = d.Mechanics;
        c.AuthorNotes = d.AuthorNotes;
        c.Host = d.Kind == CharacterKind.Host ? d.Host ?? new HostProfile() : new HostProfile();

        c.Lines.Clear();
        var order = 0;
        foreach (var l in d.Lines.Where(l => !string.IsNullOrWhiteSpace(l.Text)))
            c.Lines.Add(new CharacterLine { Situation = l.Situation, Text = l.Text.Trim(), Direction = string.IsNullOrWhiteSpace(l.Direction) ? null : l.Direction.Trim(), Order = order++ });

        c.Tells.Clear();
        order = 0;
        foreach (var t in d.Tells.Where(t => !string.IsNullOrWhiteSpace(t.Description)))
            c.Tells.Add(new CharacterTell { State = t.State, Description = t.Description.Trim(), Fakeable = t.Fakeable, VisibleWithoutDossier = t.VisibleWithoutDossier, Order = order++ });
    }

    public static void ApplyRelations(this Character c, CharacterDocument d, IReadOnlyDictionary<string, int> idsByCode)
    {
        c.Relations.Clear();
        foreach (var r in d.Relations)
        {
            if (!idsByCode.TryGetValue(r.To.Trim().ToUpperInvariant(), out var toId) || toId == c.Id) continue;
            if (c.Relations.Any(x => x.ToCharacterId == toId)) continue;
            c.Relations.Add(new CharacterRelation { ToCharacterId = toId, Value = Math.Clamp(r.Value, -100, 100), Story = r.Story.Trim(), Effect = r.Effect.Trim() });
        }
    }
}
