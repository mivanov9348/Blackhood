using System.Security.Claims;
using Blackhood.Api.Characters;
using Blackhood.Data;
using Blackhood.Data.Characters;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Blackhood.Api.Endpoints;

public sealed record Patron(string Code, string Name, string Title, int PortraitHue, bool HasPortrait, string Doing);

public sealed record RecentEvening(Guid Id, string Game, DateTime StartedAt, bool Finished, int BuyIn, int Payout);

public sealed record TavernResponse(int Gold, IReadOnlyList<Patron> Patrons, Guid? UnfinishedGifts, Guid? UnfinishedBones, IReadOnlyList<RecentEvening> Recent);

public static class TavernEndpoints
{
    /// <summary>The main hall: your purse, who is around tonight, and an evening you left unfinished.</summary>
    public static void MapTavernEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/tavern", async (ClaimsPrincipal principal, UserManager<AppUser> users, BlackhoodDbContext db, TableCatalog catalog) =>
        {
            var user = await users.GetUserAsync(principal);
            if (user is null) return Results.Unauthorized();

            var all = await catalog.GetPatronsAsync();
            var rng = new Random(DateTime.UtcNow.Hour * 31 + user.Id.GetHashCode());
            var patrons = all.OrderBy(_ => rng.Next()).Take(4).Select(c =>
            {
                var habit = c.Tells.OrderBy(t => t.Order).FirstOrDefault(t => t.VisibleWithoutDossier)?.Description ?? "";
                return new Patron(c.Code, c.Name, c.Title, c.Appearance.PortraitHue, TableCatalog.HasPortrait(c), habit);
            }).ToList();

            var games = await db.Matches.AsNoTracking()
                .Where(m => m.UserId == user.Id && (m.Game == GameKinds.Gifts || m.Game == GameKinds.Bones))
                .OrderByDescending(m => m.StartedAt)
                .Take(8)
                .Select(m => new RecentEvening(m.Id, m.Game, m.StartedAt, m.Settled, m.BuyIn, m.Payout))
                .ToListAsync();

            return Results.Ok(new TavernResponse(user.Gold, patrons,
                games.FirstOrDefault(g => !g.Finished && g.Game == GameKinds.Gifts)?.Id,
                games.FirstOrDefault(g => !g.Finished && g.Game == GameKinds.Bones)?.Id,
                games));
        }).RequireAuthorization();
    }
}
