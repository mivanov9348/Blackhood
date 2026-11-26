using Blackhood.Engine.Gifts;
using Xunit.Abstractions;
using static Blackhood.Engine.Tests.Fixtures;

namespace Blackhood.Engine.Tests;

/// <summary>Whole evenings: AI against AI, and a scripted "human" taking random legal actions.</summary>
public class GiftsSimulationTests(ITestOutputHelper output)
{
    [Fact]
    public void Thousands_of_ai_evenings_finish_and_both_sides_can_win()
    {
        const int games = 3000;
        int assassinWins = 0, guestWins = 0, slain = 0, deaths = 0, amuletsSold = 0, doors = 0, mirrors = 0;
        var byCandle = new int[GiftsRules.Candles + 1];

        for (var seed = 1; seed <= games; seed++)
        {
            var m = GiftsGame.Create(null, seed, Host, Guests(), new HashSet<string>());
            Assert.Equal(GiftsPhase.Finished, m.Phase);
            if (m.Winner == Winner.Assassin) assassinWins++; else guestWins++;
            if (m.SlayerSeat is not null) slain++;
            deaths += m.Deaths;
            byCandle[m.Candle]++;
            amuletsSold += GiftsRules.Chest[ItemType.Amulet] - m.ChestStock[ItemType.Amulet];
            doors += m.Events.Count(e => e.Kind == "door");
            mirrors += m.Events.Count(e => e.Kind == "mirror");
        }

        output.WriteLine($"{games} evenings, 5 players: Assassin wins {100.0 * assassinWins / games:F1}%, Guests {100.0 * guestWins / games:F1}%");
        output.WriteLine($"Assassin slain by mirror/poison: {100.0 * slain / games:F1}%   avg deaths {1.0 * deaths / games:F2}");
        output.WriteLine($"Ended on candle: {string.Join("  ", Enumerable.Range(1, GiftsRules.Candles).Select(c => $"{c}:{byCandle[c]}"))}");
        output.WriteLine($"Per evening: {1.0 * amuletsSold / games:F2} amulets sold, {1.0 * doors / games:F2} doors closed, {1.0 * mirrors / games:F2} mirrors raised");

        Assert.InRange(assassinWins, games * 0.15, games * 0.85);
    }

    [Fact]
    public void The_same_seed_plays_the_same_evening()
    {
        var a = GiftsGame.Create(null, 42, Host, Guests(), new HashSet<string>());
        var b = GiftsGame.Create(null, 42, Host, Guests(), new HashSet<string>());
        Assert.Equal(a.Events.Select(e => e.Text), b.Events.Select(e => e.Text));
    }

    [Theory]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    public void Every_table_size_plays_to_the_end(int players)
    {
        for (var seed = 1; seed <= 300; seed++)
            Assert.Equal(GiftsPhase.Finished, GiftsGame.Create(null, seed, Host, Guests(), new HashSet<string>(), players).Phase);
    }

    [Fact]
    public void A_scripted_human_can_play_every_phase_to_the_end()
    {
        var phasesSeen = new HashSet<GiftsPhase>();
        for (var seed = 1; seed <= 400; seed++)
        {
            var m = HumanMatch(seed);
            var rng = new Rng(seed * 7919);
            for (var step = 0; step < 500 && m.Phase != GiftsPhase.Finished; step++)
            {
                phasesSeen.Add(m.Phase);
                Act(m, rng);
            }
            Assert.Equal(GiftsPhase.Finished, m.Phase);
            var v = GiftsView.ForSeat(m, 0);
            Assert.NotNull(v.Result);
        }
        output.WriteLine("Phases the human went through: " + string.Join(", ", phasesSeen.OrderBy(x => x)));
        Assert.Contains(GiftsPhase.Delivery, phasesSeen);
        Assert.Contains(GiftsPhase.Dawn, phasesSeen);
    }

    /// <summary>One random legal human action for the current phase.</summary>
    private static void Act(GiftsMatch m, Rng rng)
    {
        var me = m.Players[0];
        var others = m.Living.Where(p => p.Seat != 0).ToList();
        switch (m.Phase)
        {
            case GiftsPhase.Whisper:
                foreach (var o in m.Offers.Where(o => o.Seat == 0 && o.Candle == m.Candle && !o.Resolved && !o.Emergency).ToList())
                    if (me.Gold >= o.Price && rng.Chance(0.3)) GiftsGame.AcceptOffer(m, 0, o.Id);
                if (me.Has(ItemType.Monocle) && others.Count > 0 && rng.Chance(0.5))
                    GiftsGame.PlayMonocle(m, 0, me.Hand.First(i => i.Type == ItemType.Monocle).Id, rng.Pick(others).Seat);
                if (me.Gold >= GiftsRules.PriceRumor && others.Count > 0 && rng.Chance(0.2))
                    GiftsGame.BuyService(m, 0, OfferKind.Rumor, rng.Pick(others).Seat);
                GiftsGame.EndWhisper(m, 0);
                break;

            case GiftsPhase.Gifts:
                if (!me.Alive)
                {
                    GiftsGame.GhostAct(m, 0, others.Count > 0 && rng.Chance(0.5) ? rng.Pick(others).Seat : null);
                    break;
                }
                var to = rng.Pick(others).Seat;
                var roll = rng.Next(3);
                if (roll == 0 && me.Hand.Count > 0)
                    GiftsGame.SendGift(m, 0, GiftKind.Card, to, Declaration.Purse, rng.Pick(me.Hand).Id);
                else if (roll == 1 && me.Gold > 0)
                    GiftsGame.SendGift(m, 0, GiftKind.Purse, to, Declaration.Purse, purse: 1);
                else
                    GiftsGame.SendGift(m, 0, GiftKind.FreeBread, to, Declaration.Bread);
                break;

            case GiftsPhase.Delivery:
                var options = new List<DeliveryResponse> { DeliveryResponse.Accept };
                if (me.Has(ItemType.ClosedDoor)) options.Add(DeliveryResponse.CloseDoor);
                if (me.Has(ItemType.Mirror)) options.Add(DeliveryResponse.Mirror);
                GiftsGame.RespondToDelivery(m, 0, rng.Pick(options));
                break;

            case GiftsPhase.WineChoice:
                GiftsGame.ChooseWine(m, 0, rng.Pick(m.Tonight.Where(g => g.Announced).ToList()).Id);
                break;

            case GiftsPhase.LastWhisper:
                var offer = m.Offers.First(o => o is { Seat: 0, Emergency: true, Resolved: false });
                GiftsGame.AcceptOffer(m, 0, offer.Id, me.Gold >= offer.Price && rng.Chance(0.5));
                break;

            case GiftsPhase.Dawn:
                GiftsGame.SeeDawn(m, 0);
                break;

            case GiftsPhase.Discard:
                var excess = me.Hand.Count - GiftsRules.HandLimit;
                var discardable = me.Hand.Where(i => i.Type != ItemType.Dagger).ToList();
                if (discardable.Count < excess) discardable = me.Hand.ToList();
                GiftsGame.DiscardCards(m, 0, discardable.Take(excess).Select(i => i.Id).ToList());
                break;

            case GiftsPhase.Pointing:
                GiftsGame.PointAt(m, 0, rng.Pick(others).Seat);
                break;
        }
    }
}
