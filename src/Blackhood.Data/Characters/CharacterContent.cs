using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Microsoft.EntityFrameworkCore;

namespace Blackhood.Data.Characters;

/// <summary>
/// Reads and writes content/characters/*.json. The database is the source of truth;
/// files are for seeding a fresh database, backups and history in git.
/// </summary>
public static class CharacterContent
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        // Keep Cyrillic readable in the files instead of А…
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static CharacterDocument Read(string path) =>
        JsonSerializer.Deserialize<CharacterDocument>(File.ReadAllText(path), Json)
        ?? throw new InvalidDataException($"Empty character file: {path}");

    public static void Write(string folder, CharacterDocument doc)
    {
        Directory.CreateDirectory(folder);
        var copy = JsonSerializer.Deserialize<CharacterDocument>(JsonSerializer.Serialize(doc, Json), Json)!;
        copy.Version = 0; // not meaningful outside the database
        File.WriteAllText(Path.Combine(folder, $"{copy.Code}.json"), JsonSerializer.Serialize(copy, Json));
    }

    /// <summary>
    /// Inserts characters from the folder whose code is not in the database yet.
    /// Existing characters are never overwritten — the editor owns them.
    /// </summary>
    public static async Task<int> ImportMissingAsync(BlackhoodDbContext db, string folder, CancellationToken ct = default)
    {
        if (!Directory.Exists(folder)) return 0;

        var docs = Directory.GetFiles(folder, "*.json").Select(Read).ToList();
        var existing = await db.Characters.Select(c => c.Code).ToListAsync(ct);
        var toInsert = docs.Where(d => !existing.Contains(d.Code.Trim().ToUpperInvariant())).ToList();
        if (toInsert.Count == 0) return 0;

        var created = new List<(Character Entity, CharacterDocument Doc)>();
        foreach (var doc in toInsert)
        {
            var result = CharacterValidator.Validate(doc);
            if (!result.IsValid)
                throw new InvalidDataException($"{doc.Code}: {string.Join(" ", result.Errors)}");

            var entity = new Character();
            entity.ApplyDocument(doc);
            db.Characters.Add(entity);
            created.Add((entity, doc));
        }
        await db.SaveChangesAsync(ct);

        // Second pass: relations need ids of every character.
        var idsByCode = await db.Characters.ToDictionaryAsync(c => c.Code, c => c.Id, ct);
        foreach (var (entity, doc) in created)
            entity.ApplyRelations(doc, idsByCode);
        await db.SaveChangesAsync(ct);

        return created.Count;
    }
}
