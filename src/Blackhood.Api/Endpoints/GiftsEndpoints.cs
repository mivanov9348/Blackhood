using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Blackhood.Api.Characters;
using Blackhood.Data;
using Blackhood.Data.Characters;
using Blackhood.Engine.Gifts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blackhood.Api.Endpoints;

/// <summary>One human move. Only the fields the action needs are read.</summary>
public sealed record GiftsAction(
    string Type,
    int? OfferId = null,
    OfferKind? Service = null,
    int? Target = null,
    int? GiftId = null,
    int? ItemId = null,
    GiftKind? Kind = null,
    Declaration? Declaration = null,
    int? Purse = null,
    DeliveryResponse? Response = null,
    List<int>? ItemIds = null);

public sealed record GiftsResponse(GiftsView Match, int ProfileGold, int? Payout);

public static class GiftsEndpoints
{
    public const int BuyIn = GiftsRules.StartingGold;
    /// <summary>The host's reward for the guest of honour (or the winning Assassin).</summary>
    public const int WinnerReward = 10;

    private static readonly JsonSerializerOptions StateJson = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    public static void MapGiftsEndpoints(this IEndpointRouteBuilder app)
    {
        var gifts = app.MapGroup("/api/gifts").RequireAuthorization();

        // Sit down at the host's table: 10 gold from the purse, four random guests.
        gifts.MapPost("/matches", async (ClaimsPrincipal principal, UserManager<AppUser> users, BlackhoodDbContext db, TableCatalog catalog) =>
        {
            var user = await users.GetUserAsync(principal);
            if (user is null) return Results.Unauthorized();

            var host = await catalog.GetHostAsync();
            var pool = await catalog.GetGuestPoolAsync();
            if (pool.Count < 4)
                return Results.Problem("At least 4 published participants are needed.", statusCode: StatusCodes.Status409Conflict);

            var encounters = await db.PlayerEncounters.Where(e => e.UserId == user.Id).Include(e => e.Character).ToListAsync();
            var metBefore = encounters.Select(e => e.Character!.Code).ToHashSet();

            // The buy-in comes out of the purse; the house covers a player who is short.
            var buyIn = Math.Min(BuyIn, user.Gold);
            user.Gold -= buyIn;

            var seed = Random.Shared.Next();
            var match = GiftsGame.Create(user.UserName ?? "You", seed, host, pool, metBefore);

            db.Matches.Add(new MatchRecord
            {
                Id = match.Id,
                UserId = user.Id,
                Mode = MatchMode.Singleplayer,
                Game = GameKinds.Gifts,
                Seed = seed,
                BuyIn = buyIn,
                StateJson = Serialize(match),
            });

            var atTable = match.Players.Where(p => p.Guest is not null).Select(p => p.Guest!.Code).Append(host.Code).ToList();
            var characters = await db.Characters.Where(c => atTable.Contains(c.Code)).ToListAsync();
            var now = DateTime.UtcNow;
            foreach (var character in characters)
            {
                var e = encounters.FirstOrDefault(x => x.CharacterId == character.Id);
                if (e is null) db.PlayerEncounters.Add(new PlayerEncounter { UserId = user.Id, CharacterId = character.Id, Count = 1, FirstMetAt = now, LastMetAt = now });
                else { e.Count++; e.LastMetAt = now; }
            }

            await db.SaveChangesAsync();
            return Results.Ok(new GiftsResponse(GiftsView.ForSeat(match, 0), user.Gold, null));
        });

        gifts.MapGet("/matches/{id:guid}", async (Guid id, ClaimsPrincipal principal, UserManager<AppUser> users, BlackhoodDbContext db) =>
        {
            var (user, record, match) = await LoadAsync(id, principal, users, db);
            return match is null ? Results.NotFound() : Results.Ok(new GiftsResponse(GiftsView.ForSeat(match, 0), user!.Gold, record!.Settled ? record.Payout : null));
        });

        gifts.MapPost("/matches/{id:guid}/intro-complete", async (Guid id, ClaimsPrincipal principal, UserManager<AppUser> users, BlackhoodDbContext db) =>
        {
            var (user, record, match) = await LoadAsync(id, principal, users, db);
            if (match is null) return Results.NotFound();
            match.IntroDone = true;
            record!.StateJson = Serialize(match);
            await db.SaveChangesAsync();
            return Results.Ok(new GiftsResponse(GiftsView.ForSeat(match, 0), user!.Gold, null));
        });

        gifts.MapPost("/matches/{id:guid}/act", async (Guid id, GiftsAction action, ClaimsPrincipal principal, UserManager<AppUser> users, BlackhoodDbContext db) =>
        {
            var (user, record, match) = await LoadAsync(id, principal, users, db);
            if (match is null) return Results.NotFound();
            if (match.Phase == GiftsPhase.Finished) return Results.BadRequest(new { message = "The evening is over." });

            try
            {
                Apply(match, action);
            }
            catch (GiftsRuleException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }

            // Pay out once, when the evening ends.
            if (match.Phase == GiftsPhase.Finished && !record!.Settled)
            {
                var me = match.Players[0];
                var payout = 0;
                if (me.Alive)
                {
                    var score = match.Scores.FirstOrDefault(s => s.Seat == 0);
                    payout = Math.Max(0, score?.Score ?? me.Gold);
                }
                if (match.WinnerSeats.Contains(0)) payout += WinnerReward;
                record.Payout = payout;
                record.Settled = true;
                record.FinishedAt = DateTime.UtcNow;
                user!.Gold += payout;
            }

            record!.StateJson = Serialize(match);
            await db.SaveChangesAsync();
            return Results.Ok(new GiftsResponse(GiftsView.ForSeat(match, 0), user!.Gold, record.Settled ? record.Payout : null));
        });
    }

    private static void Apply(GiftsMatch m, GiftsAction a)
    {
        const int me = 0;
        switch (a.Type)
        {
            case "endWhisper": GiftsGame.EndWhisper(m, me); break;
            case "acceptOffer": GiftsGame.AcceptOffer(m, me, Need(a.OfferId), accept: true); break;
            case "declineOffer": GiftsGame.AcceptOffer(m, me, Need(a.OfferId), accept: false); break;
            case "buyService": GiftsGame.BuyService(m, me, Need(a.Service), a.Target, a.GiftId); break;
            case "monocle": GiftsGame.PlayMonocle(m, me, Need(a.ItemId), Need(a.Target)); break;
            case "sendGift": GiftsGame.SendGift(m, me, Need(a.Kind), Need(a.Target), Need(a.Declaration), a.ItemId, a.Purse ?? 0); break;
            case "ghost": GiftsGame.GhostAct(m, me, a.Target); break;
            case "respond": GiftsGame.RespondToDelivery(m, me, Need(a.Response)); break;
            case "wine": GiftsGame.ChooseWine(m, me, Need(a.GiftId)); break;
            case "seeDawn": GiftsGame.SeeDawn(m, me); break;
            case "discard": GiftsGame.DiscardCards(m, me, a.ItemIds ?? []); break;
            case "point": GiftsGame.PointAt(m, me, Need(a.Target)); break;
            default: throw new GiftsRuleException($"Unknown action '{a.Type}'.");
        }
    }

    private static T Need<T>(T? value) where T : struct =>
        value ?? throw new GiftsRuleException("Something is missing from that move.");

    private static string Serialize(GiftsMatch m) => JsonSerializer.Serialize(m, StateJson);

    private static async Task<(AppUser? User, MatchRecord? Record, GiftsMatch? Match)> LoadAsync(
        Guid id, ClaimsPrincipal principal, UserManager<AppUser> users, BlackhoodDbContext db)
    {
        var user = await users.GetUserAsync(principal);
        if (user is null) return (null, null, null);
        var record = await db.Matches.FirstOrDefaultAsync(m => m.Id == id && m.UserId == user.Id && m.Game == GameKinds.Gifts);
        if (record is null) return (user, null, null);
        try
        {
            var match = JsonSerializer.Deserialize<GiftsMatch>(record.StateJson, StateJson);
            return (user, record, match?.Host is null ? null : match);
        }
        catch (JsonException)
        {
            return (user, record, null);
        }
    }
}
