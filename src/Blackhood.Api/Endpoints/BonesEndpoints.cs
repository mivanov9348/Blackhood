using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blackhood.Api.Characters;
using Blackhood.Api.GameLogs;
using Blackhood.Data;
using Blackhood.Engine.Bones;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blackhood.Api.Endpoints;

public sealed record BonesAction(string Type, int? Quantity = null, int? Face = null);

public sealed record BonesResponse(BonesView Match, int ProfileGold, int? Payout);

public static class BonesEndpoints
{
    private static readonly JsonSerializerOptions StateJson = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static void MapBonesEndpoints(this IEndpointRouteBuilder app)
    {
        var bones = app.MapGroup("/api/bones").RequireAuthorization();

        // Sit down by the fire: 5 gold into the pot, three random guests.
        bones.MapPost("/matches", async (ClaimsPrincipal principal, UserManager<AppUser> users, BlackhoodDbContext db, TableCatalog catalog) =>
        {
            var user = await users.GetUserAsync(principal);
            if (user is null) return Results.Unauthorized();
            var pool = await catalog.GetGuestPoolAsync();
            if (pool.Count < BonesRules.Players - 1)
                return Results.Problem("Not enough published participants.", statusCode: StatusCodes.Status409Conflict);

            var buyIn = Math.Min(BonesRules.BuyIn, user.Gold);
            user.Gold -= buyIn;

            var seed = Random.Shared.Next();
            var match = BonesGame.Create(user.UserName ?? "You", seed, pool);
            db.Matches.Add(new MatchRecord
            {
                Id = match.Id,
                UserId = user.Id,
                Mode = MatchMode.Singleplayer,
                Game = GameKinds.Bones,
                Seed = seed,
                BuyIn = buyIn,
                StateJson = JsonSerializer.Serialize(match, StateJson),
            });

            var log = await GameLogWriter.ForAsync(db, match.Id, user.Id, GameKinds.Bones);
            BonesLog.Start(log, match);
            BonesLog.Delta(log, match, 0, 1);

            await db.SaveChangesAsync();
            return Results.Ok(new BonesResponse(BonesView.ForSeat(match, 0), user.Gold, null));
        });

        bones.MapGet("/matches/{id:guid}", async (Guid id, ClaimsPrincipal principal, UserManager<AppUser> users, BlackhoodDbContext db) =>
        {
            var (user, record, match) = await LoadAsync(id, principal, users, db);
            return match is null ? Results.NotFound() : Results.Ok(new BonesResponse(BonesView.ForSeat(match, 0), user!.Gold, record!.Settled ? record.Payout : null));
        });

        bones.MapPost("/matches/{id:guid}/act", async (Guid id, BonesAction action, ClaimsPrincipal principal, UserManager<AppUser> users, BlackhoodDbContext db) =>
        {
            var (user, record, match) = await LoadAsync(id, principal, users, db);
            if (match is null) return Results.NotFound();
            if (match.Phase == BonesPhase.Finished) return Results.BadRequest(new { message = "The game is over." });

            var log = await GameLogWriter.ForAsync(db, match.Id, user!.Id, GameKinds.Bones);
            var (eventsBefore, roundBefore, phaseBefore) = (match.Events.Count, match.Round, match.Phase);
            var myDice = match.Players[0].Dice.OrderBy(d => d).ToList();
            try
            {
                switch (action.Type)
                {
                    case "bid": BonesGame.Bid(match, 0, action.Quantity ?? 0, action.Face ?? 0); break;
                    case "liar": BonesGame.CallLiar(match, 0); break;
                    case "next": BonesGame.Continue(match, 0); break;
                    default: throw new BonesRuleException($"Unknown action '{action.Type}'.");
                }
            }
            catch (BonesRuleException ex)
            {
                BonesLog.Error(log, match, action, ex.Message);
                await db.SaveChangesAsync();
                return Results.BadRequest(new { message = ex.Message });
            }

            BonesLog.Action(log, match, roundBefore, phaseBefore, action, action.Type, myDice);
            BonesLog.Delta(log, match, eventsBefore, roundBefore);

            if (match.Phase == BonesPhase.Finished && !record!.Settled)
            {
                var payout = match.WinnerSeat == 0 ? match.Pot : 0;
                record.Payout = payout;
                record.Settled = true;
                record.FinishedAt = DateTime.UtcNow;
                user.Gold += payout;
                BonesLog.Result(log, match, payout);
            }

            record!.StateJson = JsonSerializer.Serialize(match, StateJson);
            await db.SaveChangesAsync();
            return Results.Ok(new BonesResponse(BonesView.ForSeat(match, 0), user.Gold, record.Settled ? record.Payout : null));
        });
    }

    private static async Task<(AppUser? User, MatchRecord? Record, BonesMatch? Match)> LoadAsync(
        Guid id, ClaimsPrincipal principal, UserManager<AppUser> users, BlackhoodDbContext db)
    {
        var user = await users.GetUserAsync(principal);
        if (user is null) return (null, null, null);
        var record = await db.Matches.FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id && m.Game == GameKinds.Bones);
        if (record is null) return (user, null, null);
        try
        {
            return (user, record, JsonSerializer.Deserialize<BonesMatch>(record.StateJson, StateJson));
        }
        catch (JsonException)
        {
            return (user, record, null);
        }
    }
}
