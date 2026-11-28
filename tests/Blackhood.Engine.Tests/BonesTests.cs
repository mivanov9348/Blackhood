using Blackhood.Engine.Bones;
using Xunit.Abstractions;
using static Blackhood.Engine.Tests.Fixtures;

namespace Blackhood.Engine.Tests;

public class BonesTests(ITestOutputHelper output)
{
    private static BonesMatch Table(int[] me, int[] a, int[] b, int[] c, int turn = 0)
    {
        var m = BonesGame.Create("You", 1, Guests());
        var all = new[] { me, a, b, c };
        for (var i = 0; i < 4; i++) m.Players[i].Dice = all[i].ToList();
        m.Events.Clear();
        m.Phase = BonesPhase.Bidding;
        m.CurrentBid = null;
        m.RoundBids.Clear();
        m.TurnSeat = turn;
        return m;
    }

    [Fact]
    public void Ones_are_wild_for_other_faces()
    {
        // fives on the table: 5,5 (me) + 1 (wild) + 5 = 4
        var m = Table([5, 5, 2], [1, 3], [5, 6], [2, 2]);
        m.CurrentBid = new Bid(4, 5, 0);
        m.TurnSeat = 0;
        BonesGame.CallLiar(m, 0);
        Assert.Equal(4, m.LastReveal!.Count);
        Assert.True(m.LastReveal.BidWasTrue);
        Assert.Equal(0, m.LastReveal.LoserSeat); // I called a true bid
    }

    [Fact]
    public void A_bid_on_ones_counts_only_ones()
    {
        var m = Table([1, 5], [1, 1], [6, 6], [2, 3]);
        m.CurrentBid = new Bid(4, 1, 2);
        m.TurnSeat = 0;
        BonesGame.CallLiar(m, 0);
        Assert.Equal(3, m.LastReveal!.Count);
        Assert.False(m.LastReveal.BidWasTrue);
        Assert.Equal(2, m.LastReveal.LoserSeat); // the bidder lied
        Assert.Equal(1, m.Players[2].Dice.Count);
    }

    [Fact]
    public void Bids_must_rise()
    {
        var m = Table([2, 3], [4, 4], [5, 5], [6, 6]);
        m.CurrentBid = new Bid(3, 4, 3);
        Assert.Throws<BonesRuleException>(() => BonesGame.Bid(m, 0, 3, 4));
        Assert.Throws<BonesRuleException>(() => BonesGame.Bid(m, 0, 2, 6));
        Assert.Throws<BonesRuleException>(() => BonesGame.Bid(m, 0, 3, 3));
        BonesGame.Bid(m, 0, 3, 5); // same count, higher face is fine
    }

    [Fact]
    public void You_cannot_call_before_anyone_bids_or_act_out_of_turn()
    {
        var m = Table([2], [3], [4], [5], turn: 0);
        Assert.Throws<BonesRuleException>(() => BonesGame.CallLiar(m, 0));
        m.TurnSeat = 2;
        Assert.Throws<BonesRuleException>(() => BonesGame.Bid(m, 0, 1, 2));
    }

    [Fact]
    public void The_view_hides_other_cups_until_they_are_lifted()
    {
        var m = BonesGame.Create("You", 5, Guests());
        var v = BonesView.ForSeat(m, 0);
        Assert.Equal(BonesRules.DicePerPlayer, v.MyDice.Count);
        Assert.True(v.LastReveal is null || v.LastReveal.Round < m.Round);
    }

    [Fact]
    public void Ai_tables_always_produce_one_winner()
    {
        var wins = new int[4];
        var rounds = 0;
        for (var seed = 1; seed <= 2000; seed++)
        {
            var m = BonesGame.Create(null, seed, Guests());
            Assert.Equal(BonesPhase.Finished, m.Phase);
            Assert.NotNull(m.WinnerSeat);
            Assert.Single(m.Players, p => !p.Out);
            wins[m.WinnerSeat!.Value]++;
            rounds += m.Round;
        }
        output.WriteLine($"2000 AI tables: avg {rounds / 2000.0:F1} rounds; wins by seat {string.Join("/", wins)}");
    }

    [Fact]
    public void A_scripted_human_plays_to_the_end()
    {
        for (var seed = 1; seed <= 300; seed++)
        {
            var m = BonesGame.Create("You", seed, Guests());
            var rng = new Rng(seed * 31);
            for (var step = 0; step < 400 && m.Phase != BonesPhase.Finished; step++)
            {
                if (m.Phase == BonesPhase.Reveal) { BonesGame.Continue(m, 0); continue; }
                if (m.TurnSeat != 0) { BonesGame.Run(m); continue; }
                var b = m.CurrentBid;
                if (b is not null && rng.Chance(0.4)) BonesGame.CallLiar(m, 0);
                else if (b is null) BonesGame.Bid(m, 0, 1, rng.Next(6) + 1);
                else if (b.Face < 6) BonesGame.Bid(m, 0, b.Quantity, b.Face + 1);
                else if (b.Quantity < m.TotalDice) BonesGame.Bid(m, 0, b.Quantity + 1, 2);
                else BonesGame.CallLiar(m, 0);
            }
            Assert.Equal(BonesPhase.Finished, m.Phase);
        }
    }
}
