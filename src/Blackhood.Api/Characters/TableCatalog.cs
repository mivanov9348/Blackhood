using Blackhood.Data;
using Blackhood.Data.Characters;
using Blackhood.Engine;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using EngineTell = Blackhood.Engine.Tell;
using EngineTellState = Blackhood.Engine.TellState;

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
            ?? throw new InvalidOperationException("No published host.");

        return new TableHost(
            host.Code,
            host.Name,
            host.Title,
            host.Appearance.PortraitHue,
            HasPortrait(host),
            Line(host, LineSituation.FirstMeeting, "Welcome. Sit down."),
            Line(host, LineSituation.MetAgain, Line(host, LineSituation.FirstMeeting, "You again.")),
            Line(host, LineSituation.IntroReplyWho, "The host."),
            Line(host, LineSituation.IntroReplyBegin, "Then let us begin."),
            Line(host, LineSituation.IntroReplySilent, "As you wish."),
            Line(host, LineSituation.RoleDeal, "This is your role."));
    }

    public async Task<List<TableGuest>> GetGuestPoolAsync(CancellationToken ct = default)
    {
        var participants = await Query().Where(c => c.Kind == CharacterKind.Participant).ToListAsync(ct);
        return participants.Select(ToGuest).ToList();
    }

    /// <summary>A few published participants "around the tavern" for the main hall.</summary>
    public async Task<List<Character>> GetPatronsAsync(CancellationToken ct = default) =>
        await Query().Where(c => c.Kind == CharacterKind.Participant).ToListAsync(ct);

    public static TableGuest ToGuest(Character c)
    {
        var first = c.Lines.Where(l => l.Situation == LineSituation.FirstMeeting).OrderBy(l => l.Order).FirstOrDefault();
        var tells = c.Tells.OrderBy(t => t.Order)
            .Select(t => new EngineTell((EngineTellState)(int)t.State, t.Description, t.Fakeable))
            .ToList();
        var gesture = c.Tells.Where(t => t.VisibleWithoutDossier).OrderBy(t => t.Order).FirstOrDefault()?.Description ?? "";
        var s = c.Skills;
        var d = c.Drives;

        return new TableGuest(
            c.Code,
            c.Name,
            c.Title,
            c.Appearance.PortraitHue,
            HasPortrait(c),
            new Traits(s.Insight, s.Calculation, s.Cunning, s.Charisma, s.Composure,
                d.Greed, d.Boldness, d.Loyalty, d.Revenge, d.Patience, d.Ambition, d.Pride, d.FearOfLoss),
            tells,
            first?.Text ?? "…",
            first?.Direction,
            Line(c, LineSituation.HostIntroduction, c.Name + "."),
            Line(c, LineSituation.MetAgain, first?.Text ?? "…"),
            Line(c, LineSituation.Death, ""),
            gesture);
    }

    private IQueryable<Character> Query() => db.Characters
        .AsNoTracking()
        .AsSplitQuery()
        .Include(c => c.Lines)
        .Include(c => c.Tells)
        .Include(c => c.Assets)
        .Where(c => c.Status == ContentStatus.Published);

    public static bool HasPortrait(Character c) =>
        c.Assets.Any(a => a.Kind == AssetKind.Portrait && a.Expression == Expression.Neutral);

    private static string Line(Character c, LineSituation s, string fallback) =>
        c.Lines.Where(l => l.Situation == s).OrderBy(l => l.Order).FirstOrDefault()?.Text ?? fallback;
}
