using Blackhood.Api.Characters;
using Blackhood.Data;
using Blackhood.Data.Characters;
using Microsoft.EntityFrameworkCore;

namespace Blackhood.Api.Endpoints;

public static class CharacterEndpoints
{
    /// <summary>Public read-only character endpoints (portraits for the table and the intro).</summary>
    public static void MapCharacterEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/characters/{code}/portrait", async (string code, BlackhoodDbContext db, ContentPaths paths, HttpContext http) =>
        {
            var asset = await db.CharacterAssets.AsNoTracking()
                .Where(a => a.Kind == AssetKind.Portrait && a.Expression == Expression.Neutral)
                .Join(db.Characters.Where(c => c.Code == code.ToUpper()), a => a.CharacterId, c => c.Id, (a, _) => a)
                .FirstOrDefaultAsync();
            if (asset is null) return Results.NotFound();

            var file = Path.Combine(paths.Portraits, asset.FileName);
            if (!File.Exists(file)) return Results.NotFound();

            http.Response.Headers.CacheControl = "no-cache";
            return Results.File(file, asset.ContentType, lastModified: asset.UploadedAt, enableRangeProcessing: false);
        });
    }
}
