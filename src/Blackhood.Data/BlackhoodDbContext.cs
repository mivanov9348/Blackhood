using Blackhood.Data.Characters;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Blackhood.Data;

public sealed class BlackhoodDbContext(DbContextOptions<BlackhoodDbContext> options)
    : IdentityDbContext<AppUser>(options)
{
    public DbSet<MatchRecord> Matches => Set<MatchRecord>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<CharacterLine> CharacterLines => Set<CharacterLine>();
    public DbSet<CharacterTell> CharacterTells => Set<CharacterTell>();
    public DbSet<CharacterRelation> CharacterRelations => Set<CharacterRelation>();
    public DbSet<CharacterAsset> CharacterAssets => Set<CharacterAsset>();
    public DbSet<PlayerEncounter> PlayerEncounters => Set<PlayerEncounter>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<AppUser>().Property(u => u.Gold).HasDefaultValue(AppUser.StartingGold);

        builder.Entity<MatchRecord>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Mode).HasConversion<string>().HasMaxLength(20);
            e.Property(m => m.StateJson).IsRequired();
            e.Property(m => m.Game).HasMaxLength(20).HasDefaultValue(GameKinds.Legacy);
            e.HasOne(m => m.User)
                .WithMany(u => u.Matches)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(m => new { m.UserId, m.StartedAt });
        });

        builder.Entity<Character>(e =>
        {
            e.HasIndex(c => c.Code).IsUnique();
            e.Property(c => c.Code).HasMaxLength(16);
            e.Property(c => c.Name).HasMaxLength(120);
            e.Property(c => c.Title).HasMaxLength(120);
            e.Property(c => c.Nickname).HasMaxLength(200);
            e.Property(c => c.Gender).HasMaxLength(40);
            e.Property(c => c.Age).HasMaxLength(80);
            e.Property(c => c.Archetype).HasMaxLength(80);
            e.Property(c => c.Kind).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.Rarity).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.Status).HasConversion<string>().HasMaxLength(20);
            e.Property(c => c.Version).IsConcurrencyToken();

            // Numbers → plain columns (Skills_Insight, Drives_Greed, …)
            e.ComplexProperty(c => c.Skills);
            e.ComplexProperty(c => c.Drives);

            // Authored text → one JSON column per section
            e.ComplexProperty(c => c.Origin, b => b.ToJson());
            e.ComplexProperty(c => c.Appearance, b => b.ToJson());
            e.ComplexProperty(c => c.Backstory, b => b.ToJson());
            e.ComplexProperty(c => c.Psychology, b => b.ToJson());
            e.ComplexProperty(c => c.Behavior, b => b.ToJson());
            e.ComplexProperty(c => c.Motives, b => b.ToJson());
            e.ComplexProperty(c => c.Speech, b => b.ToJson());
            e.ComplexProperty(c => c.Mechanics, b => b.ToJson());
            e.ComplexProperty(c => c.AuthorNotes, b => b.ToJson());
            e.ComplexProperty(c => c.Host, b => b.ToJson());

            e.HasMany(c => c.Lines).WithOne().HasForeignKey(l => l.CharacterId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(c => c.Tells).WithOne().HasForeignKey(t => t.CharacterId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(c => c.Assets).WithOne().HasForeignKey(a => a.CharacterId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(c => c.Relations).WithOne().HasForeignKey(r => r.FromCharacterId).OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<CharacterLine>(e =>
        {
            e.Property(l => l.Situation).HasConversion<string>().HasMaxLength(40);
            e.Property(l => l.Text).HasMaxLength(1000);
            e.Property(l => l.Direction).HasMaxLength(300);
            e.HasIndex(l => new { l.CharacterId, l.Situation });
        });

        builder.Entity<CharacterTell>(e =>
        {
            e.Property(t => t.State).HasConversion<string>().HasMaxLength(40);
            e.Property(t => t.Description).HasMaxLength(500);
        });

        builder.Entity<CharacterRelation>(e =>
        {
            e.HasOne(r => r.To).WithMany().HasForeignKey(r => r.ToCharacterId).OnDelete(DeleteBehavior.NoAction);
            e.HasIndex(r => new { r.FromCharacterId, r.ToCharacterId }).IsUnique();
            e.Property(r => r.Story).HasMaxLength(500);
            e.Property(r => r.Effect).HasMaxLength(500);
        });

        builder.Entity<CharacterAsset>(e =>
        {
            e.Property(a => a.Kind).HasConversion<string>().HasMaxLength(20);
            e.Property(a => a.Expression).HasConversion<string>().HasMaxLength(20);
            e.Property(a => a.FileName).HasMaxLength(200);
            e.Property(a => a.ContentType).HasMaxLength(100);
            e.HasIndex(a => new { a.CharacterId, a.Kind, a.Expression }).IsUnique();
        });

        builder.Entity<PlayerEncounter>(e =>
        {
            e.HasKey(p => new { p.UserId, p.CharacterId });
            e.HasOne<AppUser>().WithMany().HasForeignKey(p => p.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(p => p.Character).WithMany().HasForeignKey(p => p.CharacterId).OnDelete(DeleteBehavior.Cascade);
        });
    }
}
