namespace Blackhood.Engine;

public enum RoleType
{
    Assassin,
    Inquisitor,
    Thief,
    Merchant,
    Priest,
    Sold,
    Fool,
}

public static class RoleSets
{
    /// <summary>Prototype: fixed 5 roles (no Fool, no Sold) — see concept v0.4.</summary>
    public static readonly IReadOnlyList<RoleType> Prototype =
    [
        RoleType.Assassin,
        RoleType.Inquisitor,
        RoleType.Thief,
        RoleType.Merchant,
        RoleType.Priest,
    ];
}
