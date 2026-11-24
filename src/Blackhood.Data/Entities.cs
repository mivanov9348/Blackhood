using Microsoft.AspNetCore.Identity;

namespace Blackhood.Data;

public sealed class AppUser : IdentityUser
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<MatchRecord> Matches { get; set; } = [];
}

public enum MatchMode
{
    Singleplayer,
    Multiplayer,
}

/// <summary>One played (or ongoing) match. The engine state is stored as JSON.</summary>
public sealed class MatchRecord
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = "";
    public AppUser? User { get; set; }
    public MatchMode Mode { get; set; }
    public int Seed { get; set; }
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? FinishedAt { get; set; }
    public string StateJson { get; set; } = "";
}
