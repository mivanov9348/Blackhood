using System.Security.Claims;
using System.Text.Json;
using Blackhood.Api.Characters;
using Blackhood.Data;
using Blackhood.Data.Characters;
using Blackhood.Engine;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blackhood.Api.Endpoints;

public static class SingleplayerEndpoints
{
    private static readonly JsonSerializerOptions StateJson = new(JsonSerializerDefaults.Web);

    public static void MapSingleplayerEndpoints(this IEndpointRouteBuilder app)
    {
        var sp = app.MapGroup("/api/singleplayer").RequireAuthorization();

        // Start a new evening at the tavern table.
        sp.MapPost("/matches", async (ClaimsPrincipal principal, UserManager<AppUser> users, BlackhoodDbContext db, TableCatalog catalog) =>
        {
            var user = await users.GetUserAsync(principal);
            if (user is null) return Results.Unauthorized();

            var host = await catalog.GetHostAsync();
            var pool = await catalog.GetGuestPoolAsync();
            if (pool.Count < MatchFactory.Opponents)
                return Results.Problem($"Нужни са поне {MatchFactory.Opponents} публикувани участника.", statusCode: StatusCodes.Status409Conflict);

            var encounters = await db.PlayerEncounters
                .Where(e => e.UserId == user.Id)
                .Include(e => e.Character)
                .ToListAsync();
            var metBefore = encounters.Select(e => e.Character!.Code).ToHashSet();

            var seed = Random.Shared.Next();
            var match = MatchFactory.CreateSingleplayer(user.UserName ?? "Ти", seed, host, pool, metBefore);

            db.Matches.Add(new MatchRecord
            {
                Id = match.Id,
                UserId = user.Id,
                Mode = MatchMode.Singleplayer,
                Seed = seed,
                StateJson = JsonSerializer.Serialize(match, StateJson),
            });

            // Remember who the player sat with — short intro next time, dossier later.
            var atTable = match.Seats.Where(s => s.Guest is not null).Select(s => s.Guest!.Code).Append(host.Code).ToList();
            var characters = await db.Characters.Where(c => atTable.Contains(c.Code)).ToListAsync();
            var now = DateTime.UtcNow;
            foreach (var character in characters)
            {
                var encounter = encounters.FirstOrDefault(e => e.CharacterId == character.Id);
                if (encounter is null)
                    db.PlayerEncounters.Add(new PlayerEncounter { UserId = user.Id, CharacterId = character.Id, Count = 1, FirstMetAt = now, LastMetAt = now });
                else
                {
                    encounter.Count++;
                    encounter.LastMetAt = now;
                }
            }

            await db.SaveChangesAsync();
            return Results.Ok(MatchView.ForSeat(match, 0));
        });

        sp.MapGet("/matches/{id:guid}", async (Guid id, ClaimsPrincipal principal, UserManager<AppUser> users, BlackhoodDbContext db) =>
        {
            var record = await FindAsync(id, principal, users, db, tracking: false);
            var match = record is null ? null : Load(record);
            return match is null ? Results.NotFound() : Results.Ok(MatchView.ForSeat(match, 0));
        });

        // The arrival scene was watched or skipped.
        sp.MapPost("/matches/{id:guid}/intro-complete", async (Guid id, ClaimsPrincipal principal, UserManager<AppUser> users, BlackhoodDbContext db) =>
        {
            var record = await FindAsync(id, principal, users, db, tracking: true);
            var match = record is null ? null : Load(record);
            if (record is null || match is null) return Results.NotFound();

            match.IntroDone = true;
            record.StateJson = JsonSerializer.Serialize(match, StateJson);
            await db.SaveChangesAsync();
            return Results.Ok(MatchView.ForSeat(match, 0));
        });
    }

    private static async Task<MatchRecord?> FindAsync(Guid id, ClaimsPrincipal principal, UserManager<AppUser> users, BlackhoodDbContext db, bool tracking)
    {
        var userId = users.GetUserId(principal);
        var query = tracking ? db.Matches : db.Matches.AsNoTracking();
        return await query.FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);
    }

    /// <summary>Matches saved before the character system have no host — treat them as gone.</summary>
    private static MatchState? Load(MatchRecord record)
    {
        var match = JsonSerializer.Deserialize<MatchState>(record.StateJson, StateJson);
        return match?.Host is null ? null : match;
    }
}
