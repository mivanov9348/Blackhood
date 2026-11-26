using Blackhood.Api.Characters;
using Blackhood.Data;
using Blackhood.Data.Characters;
using Microsoft.EntityFrameworkCore;

namespace Blackhood.Api.Endpoints;

public sealed record CharacterSummary(
    string Code, string Name, string Title, CharacterKind Kind, Rarity Rarity, ContentStatus Status,
    int PortraitHue, bool HasPortrait, int WarningCount, DateTime UpdatedAt);

public sealed record CharacterEditorResponse(CharacterDocument Document, ValidationResult Validation, bool HasPortrait, int? SkillBudget);

public static class AdminEndpoints
{
    private static readonly string[] AllowedImageTypes = ["image/png", "image/jpeg", "image/webp"];
    private const long MaxPortraitBytes = 5 * 1024 * 1024;

    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        var admin = app.MapGroup("/api/admin").RequireAdmin();

        admin.MapGet("/characters", async (BlackhoodDbContext db) =>
        {
            var all = await LoadAll(db).ToListAsync();
            var codes = all.ToDictionary(c => c.Id, c => c.Code);
            return all
                .OrderBy(c => c.Kind).ThenBy(c => c.Code)
                .Select(c => new CharacterSummary(
                    c.Code, c.Name, c.Title, c.Kind, c.Rarity, c.Status, c.Appearance.PortraitHue,
                    c.Assets.Any(a => a.Kind == AssetKind.Portrait),
                    CharacterValidator.Validate(c.ToDocument(codes)).Warnings.Count,
                    c.UpdatedAt))
                .ToList();
        });

        admin.MapGet("/characters/{code}", async (string code, BlackhoodDbContext db) =>
        {
            var all = await LoadAll(db).ToListAsync();
            var c = all.FirstOrDefault(x => x.Code == code.ToUpperInvariant());
            return c is null ? Results.NotFound() : Results.Ok(EditorResponse(c, all));
        });

        // Live validation while editing — nothing is saved.
        admin.MapPost("/characters/validate", (CharacterDocument doc) =>
            Results.Ok(new { validation = CharacterValidator.Validate(doc), skillBudget = CharacterValidator.SkillBudget(doc.Rarity) }));

        admin.MapPost("/characters", async (CharacterDocument doc, BlackhoodDbContext db) =>
        {
            var validation = CharacterValidator.Validate(doc);
            if (!validation.IsValid) return Results.UnprocessableEntity(validation);

            var code = doc.Code.Trim().ToUpperInvariant();
            if (await db.Characters.AnyAsync(c => c.Code == code))
                return Results.Conflict(new { message = $"A character with code {code} already exists." });

            var entity = new Character();
            entity.ApplyDocument(doc);
            db.Characters.Add(entity);
            await db.SaveChangesAsync();

            entity.ApplyRelations(doc, await db.Characters.ToDictionaryAsync(c => c.Code, c => c.Id));
            await db.SaveChangesAsync();

            var all = await LoadAll(db).ToListAsync();
            return Results.Ok(EditorResponse(all.First(c => c.Id == entity.Id), all));
        });

        admin.MapPut("/characters/{code}", async (string code, CharacterDocument doc, BlackhoodDbContext db) =>
        {
            code = code.ToUpperInvariant();
            if (!string.Equals(doc.Code.Trim(), code, StringComparison.OrdinalIgnoreCase))
                return Results.BadRequest(new { message = "A character's code cannot be changed." });

            var validation = CharacterValidator.Validate(doc);
            if (!validation.IsValid) return Results.UnprocessableEntity(validation);

            var entity = await db.Characters
                .AsSplitQuery()
                .Include(c => c.Lines).Include(c => c.Tells).Include(c => c.Relations)
                .FirstOrDefaultAsync(c => c.Code == code);
            if (entity is null) return Results.NotFound();
            if (entity.Version != doc.Version)
                return Results.Conflict(new { message = "Someone else changed this character in the meantime. Reload the page." });

            entity.ApplyDocument(doc);
            entity.ApplyRelations(doc, await db.Characters.ToDictionaryAsync(c => c.Code, c => c.Id));
            entity.Version++;
            entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            var all = await LoadAll(db).ToListAsync();
            return Results.Ok(EditorResponse(all.First(c => c.Id == entity.Id), all));
        });

        admin.MapDelete("/characters/{code}", async (string code, BlackhoodDbContext db, ContentPaths paths) =>
        {
            var entity = await db.Characters.Include(c => c.Assets).FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant());
            if (entity is null) return Results.NotFound();

            // Relations pointing AT this character are NoAction in SQL — remove them first.
            await db.CharacterRelations.Where(r => r.ToCharacterId == entity.Id).ExecuteDeleteAsync();
            foreach (var asset in entity.Assets)
                File.Delete(Path.Combine(paths.Portraits, asset.FileName));
            db.Characters.Remove(entity);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        // Writes the character back to content/characters/<CODE>.json (for git history / seeding).
        admin.MapPost("/characters/{code}/export", async (string code, BlackhoodDbContext db, ContentPaths paths) =>
        {
            var all = await LoadAll(db).ToListAsync();
            var c = all.FirstOrDefault(x => x.Code == code.ToUpperInvariant());
            if (c is null) return Results.NotFound();
            CharacterContent.Write(paths.Characters, c.ToDocument(all.ToDictionary(x => x.Id, x => x.Code)));
            return Results.Ok(new { file = $"content/characters/{c.Code}.json" });
        });

        admin.MapPost("/characters/export-all", async (BlackhoodDbContext db, ContentPaths paths) =>
        {
            var all = await LoadAll(db).ToListAsync();
            var codes = all.ToDictionary(x => x.Id, x => x.Code);
            foreach (var c in all) CharacterContent.Write(paths.Characters, c.ToDocument(codes));
            return Results.Ok(new { count = all.Count });
        });

        admin.MapPost("/characters/{code}/portrait", async (string code, IFormFile file, BlackhoodDbContext db, ContentPaths paths) =>
        {
            if (file.Length is 0 or > MaxPortraitBytes)
                return Results.BadRequest(new { message = "The file must be 5 MB or smaller." });
            if (!AllowedImageTypes.Contains(file.ContentType))
                return Results.BadRequest(new { message = "PNG, JPEG and WebP are allowed." });

            var entity = await db.Characters.Include(c => c.Assets).FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant());
            if (entity is null) return Results.NotFound();

            var ext = file.ContentType switch { "image/png" => ".png", "image/webp" => ".webp", _ => ".jpg" };
            var fileName = $"{entity.Code}-neutral{ext}";
            Directory.CreateDirectory(paths.Portraits);

            var existing = entity.Assets.FirstOrDefault(a => a.Kind == AssetKind.Portrait && a.Expression == Expression.Neutral);
            if (existing is not null && existing.FileName != fileName)
                File.Delete(Path.Combine(paths.Portraits, existing.FileName));

            await using (var stream = File.Create(Path.Combine(paths.Portraits, fileName)))
                await file.CopyToAsync(stream);

            if (existing is null)
                entity.Assets.Add(new CharacterAsset { Kind = AssetKind.Portrait, Expression = Expression.Neutral, FileName = fileName, ContentType = file.ContentType });
            else
            {
                existing.FileName = fileName;
                existing.ContentType = file.ContentType;
                existing.UploadedAt = DateTime.UtcNow;
            }
            entity.UpdatedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).DisableAntiforgery();

        admin.MapDelete("/characters/{code}/portrait", async (string code, BlackhoodDbContext db, ContentPaths paths) =>
        {
            var entity = await db.Characters.Include(c => c.Assets).FirstOrDefaultAsync(c => c.Code == code.ToUpperInvariant());
            var asset = entity?.Assets.FirstOrDefault(a => a.Kind == AssetKind.Portrait && a.Expression == Expression.Neutral);
            if (entity is null || asset is null) return Results.NotFound();
            File.Delete(Path.Combine(paths.Portraits, asset.FileName));
            entity.Assets.Remove(asset);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    private static IQueryable<Character> LoadAll(BlackhoodDbContext db) => db.Characters
        .AsNoTracking()
        .AsSplitQuery()
        .Include(c => c.Lines)
        .Include(c => c.Tells)
        .Include(c => c.Relations)
        .Include(c => c.Assets);

    private static CharacterEditorResponse EditorResponse(Character c, IReadOnlyList<Character> all)
    {
        var doc = c.ToDocument(all.ToDictionary(x => x.Id, x => x.Code));
        return new CharacterEditorResponse(
            doc,
            CharacterValidator.Validate(doc),
            c.Assets.Any(a => a.Kind == AssetKind.Portrait),
            CharacterValidator.SkillBudget(c.Rarity));
    }
}
