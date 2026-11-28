namespace Blackhood.Engine.Bones;

/// <summary>What one seat sees: its own dice, everyone's dice count, and the cups only when lifted.</summary>
public sealed record BonesView(
    Guid Id,
    int Round,
    BonesPhase Phase,
    int Pot,
    int TotalDice,
    int TurnSeat,
    Bid? CurrentBid,
    IReadOnlyList<Bid> RoundBids,
    IReadOnlyList<int> MyDice,
    IReadOnlyList<BonesSeatView> Players,
    RevealReport? LastReveal,
    IReadOnlyList<BonesEvent> Events,
    int? WinnerSeat,
    double? HintChance)
{
    public static BonesView ForSeat(BonesMatch m, int seat)
    {
        var me = m.Players[seat];
        double? hint = null;
        if (m.Phase == BonesPhase.Bidding && m.CurrentBid is { } b && m.TurnSeat == seat && !me.Out)
        {
            // Plain arithmetic the player could do themself: the chance the current bid is true.
            var own = me.Dice.Count(d => d == b.Face || (b.Face != BonesRules.WildFace && d == BonesRules.WildFace));
            hint = BonesAi.AtLeast(m.TotalDice - me.Dice.Count, b.Quantity - own, b.Face == BonesRules.WildFace ? 1.0 / 6 : 2.0 / 6);
        }

        return new BonesView(
            m.Id, m.Round, m.Phase, m.Pot, m.TotalDice, m.TurnSeat, m.CurrentBid, m.RoundBids.ToList(),
            me.Dice.OrderBy(d => d).ToList(),
            m.Players.Select(p => new BonesSeatView(
                p.Seat, p.Guest?.Code ?? "", p.Name, p.Guest?.Title ?? "You", p.Guest?.PortraitHue ?? 30, p.Guest?.HasPortrait ?? false,
                p.Seat == seat, p.Dice.Count, p.Out)).ToList(),
            m.Phase is BonesPhase.Reveal or BonesPhase.Finished ? m.LastReveal : m.LastReveal is { } r && r.Round < m.Round ? r : null,
            m.Events,
            m.WinnerSeat,
            hint);
    }
}

public sealed record BonesSeatView(int Seat, string Code, string Name, string Title, int PortraitHue, bool HasPortrait, bool IsMe, int DiceCount, bool Out);
