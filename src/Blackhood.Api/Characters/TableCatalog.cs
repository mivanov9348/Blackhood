using Blackhood.Data;
using Blackhood.Data.Characters;
using Blackhood.Engine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Blackhood.Api.Characters;

/// <summary>Turns published database characters into the engine's table records.</summary>
public sealed class TableCatalog(BlackhoodDbContext db, IOptions<ContentOptions> options)
{
    public async Task<TableHost> GetHostAsync(CancellationToken ct = default)
    {
        var code = options.Value.DefaultHost;
        var host = await Query()
            .Where(c => c.Kind == CharacterKind.Host)
            .OrderBy(c => c.Code == code ? 0 : 1).ThenBy(c => c.Code)
            .FirstOrDefaultAsync(ct)
            ?? throw new InvalidOperationException("Няма публикуван домакин.");

        return new TableHost(
            host.Code,
            host.Name,
            host.Title,
            host.Appearance.PortraitHue,
            HasPortrait(host),
            Line(host, LineSituation.FirstMeeting, "Добре дошли. Седнете."),
            Line(host, LineSituation.MetAgain, Line(host, LineSituation.FirstMeeting, "Отново вие.")),
            Line(host, LineSituation.IntroReplyWho, "Домакинът."),
            Line(host, LineSituation.IntroReplyBegin, "Тогава да започваме."),
            Line(host, LineSituation.IntroReplySilent, "Както желаете."),
            Line(host, LineSituation.RoleDeal, "Това е вашата роля."));
    }

    public async Task<List<TableGuest>> GetGuestPoolAsync(CancellationToken ct = default)
    {
        var participants = await Query().Where(c => c.Kind == CharacterKind.Participant).ToListAsync(ct);
        return participants.Select(c =>
        {
            var first = c.Lines.Where(l => l.Situation == LineSituation.FirstMeeting).OrderBy(l => l.Order).FirstOrDefault();
            var gesture = c.Tells.Where(t => t.VisibleWithoutDossier).OrderBy(t => t.Order).FirstOrDefault()?.Description ?? "";
            return new TableGuest(
                c.Code,
                c.Name,
                c.Title,
                c.Archetype,
                c.Appearance.PortraitHue,
                HasPortrait(c),
                first?.Text ?? "…",
                first?.Direction,
                Line(c, LineSituation.HostIntroduction, c.Name + "."),
                Line(c, LineSituation.MetAgain, first?.Text ?? "…"),
                gesture);
        }).ToList();
    }

    private IQueryable<Character> Query() => db.Characters
        .AsNoTracking()
        .AsSplitQuery()
        .Include(c => c.Lines)
        .Include(c => c.Tells)
        .Include(c => c.Assets)
        .Where(c => c.Status == ContentStatus.Published);

    private static bool HasPortrait(Character c) =>
        c.Assets.Any(a => a.Kind == AssetKind.Portrait && a.Expression == Expression.Neutral);

    private static string Line(Character c, LineSituation s, string fallback) =>
        c.Lines.Where(l => l.Situation == s).OrderBy(l => l.Order).FirstOrDefault()?.Text ?? fallback;
}
