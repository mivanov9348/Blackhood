using Blackhood.Api.Characters;
using Blackhood.Data;
using Microsoft.EntityFrameworkCore;

namespace Blackhood.Api.Endpoints;

public static class LogEndpoints
{
    /// <summary>Admin-only access to the play-by-play logs of test games.</summary>
    public static void MapLogEndpoints(this IEndpointRouteBuilder app)
    {
        var logs = app.MapGroup("/api/admin/logs").RequireAdmin();

        // Recent matches: who played, which game, how it started and ended.
        logs.MapGet("", async (BlackhoodDbContext db, int? take) =>
        {
            var n = Math.Clamp(take ?? 30, 1, 200);
            var matches = await db.GameLogs.AsNoTracking()
                .GroupBy(l => new { l.MatchId, l.Game, l.UserId })
                .Select(g => new { g.Key.MatchId, g.Key.Game, g.Key.UserId, Started = g.Min(l => l.At), Last = g.Max(l => l.At), Lines = g.Count() })
                .OrderByDescending(x => x.Last)
                .Take(n)
                .ToListAsync();

            var ids = matches.Select(x => x.MatchId).ToList();
            var edges = await db.GameLogs.AsNoTracking()
                .Where(l => ids.Contains(l.MatchId) && (l.Kind == "start" || l.Kind == "result"))
                .Select(l => new { l.MatchId, l.Kind, l.Text })
                .ToListAsync();
            var userIds = matches.Select(x => x.UserId).Distinct().ToList();
            var names = await db.Users.Where(u => userIds.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u.UserName);

            return matches.Select(x => new
            {
                x.MatchId,
                x.Game,
                Player = names.GetValueOrDefault(x.UserId),
                x.Started,
                x.Last,
                x.Lines,
                Start = edges.FirstOrDefault(e => e.MatchId == x.MatchId && e.Kind == "start")?.Text,
                Result = edges.FirstOrDefault(e => e.MatchId == x.MatchId && e.Kind == "result")?.Text,
            });
        });

        logs.MapGet("/{matchId:guid}", async (Guid matchId, BlackhoodDbContext db) =>
            await db.GameLogs.AsNoTracking()
                .Where(l => l.MatchId == matchId)
                .OrderBy(l => l.Seq)
                .Select(l => new { l.Seq, l.At, l.Round, l.Phase, l.Kind, l.Text, l.Data })
                .ToListAsync());
    }
}
