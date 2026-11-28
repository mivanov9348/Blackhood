using System.Text.Json;
using System.Text.Json.Serialization;
using Blackhood.Data;
using Microsoft.EntityFrameworkCore;

namespace Blackhood.Api.GameLogs;

/// <summary>
/// Appends play-by-play lines for one match to the GameLogs table. Lines are added to the
/// DbContext and saved together with the match state, so a log line never describes a move
/// that was not saved.
/// </summary>
public sealed class GameLogWriter
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    private readonly BlackhoodDbContext _db;
    private readonly Guid _matchId;
    private readonly string _userId;
    private readonly string _game;
    private int _seq;

    private GameLogWriter(BlackhoodDbContext db, Guid matchId, string userId, string game, int seq)
    {
        _db = db;
        _matchId = matchId;
        _userId = userId;
        _game = game;
        _seq = seq;
    }

    public static async Task<GameLogWriter> ForAsync(BlackhoodDbContext db, Guid matchId, string userId, string game)
    {
        var last = await db.GameLogs.Where(l => l.MatchId == matchId).MaxAsync(l => (int?)l.Seq) ?? 0;
        return new GameLogWriter(db, matchId, userId, game, last);
    }

    public void Add(int round, string phase, string kind, string text, object? data = null) =>
        _db.GameLogs.Add(new GameLog
        {
            MatchId = _matchId,
            UserId = _userId,
            Game = _game,
            Seq = ++_seq,
            Round = round,
            Phase = phase,
            Kind = kind,
            Text = text.Length > 4000 ? text[..4000] : text,
            Data = data is null ? null : JsonSerializer.Serialize(data, Json),
        });
}
