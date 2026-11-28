using Blackhood.Engine.Gifts;
using static Blackhood.Engine.Tests.Fixtures;

namespace Blackhood.Engine.Tests;

public class GiftsRulesTests
{
    [Fact]
    public void Setup_deals_roles_gold_and_cards()
    {
        var m = HumanMatch();

        Assert.Equal(5, m.Players.Count);
        Assert.Single(m.Players, p => p.Role == Role.Assassin);
        Assert.Equal(2, m.Quota);
        Assert.All(m.Players, p => Assert.Equal(GiftsRules.StartingGold + GiftsRules.IncomePerCandle, p.Gold));
        // 3 dealt + 1 drawn at the first candle — the same for everyone, so hand size never betrays the Assassin
        Assert.All(m.Players, p => Assert.Equal(4, p.Hand.Count));
        Assert.True(m.Players.Single(p => p.Role == Role.Assassin).Count(ItemType.Dagger) >= GiftsRules.AssassinExtraDaggers);
        Assert.Equal(GiftsRules.Deck.Values.Sum(), m.Deck.Count + m.Players.Sum(p => p.Hand.Count) - GiftsRules.AssassinExtraDaggers);
        Assert.Equal(GiftsRules.Chest[ItemType.Amulet], m.ChestStock[ItemType.Amulet]);
        Assert.Equal(GiftsPhase.Whisper, m.Phase); // waiting for the human
    }

    [Fact]
    public void Seven_players_have_two_assassins_and_a_quota_of_three()
    {
        var m = GiftsGame.Create(null, 3, Host, Guests(), new HashSet<string>(), players: 7);
        Assert.Equal(2, m.Players.Count(p => p.Role == Role.Assassin));
        Assert.Equal(3, m.Quota);
    }

    [Fact]
    public void Assassin_dagger_kills_a_guest_at_dawn()
    {
        var m = Scenario();
        Deliver(m, from: 1, to: 2, ItemType.Dagger);
        GiftsGame.Dawn(m);

        Assert.False(m.Players[2].Alive);
        Assert.Equal(DeathCause.Dagger, m.Players[2].DeathCause);
        Assert.Equal(1, m.Deaths);
        Assert.Equal(0, m.Players[2].Gold); // the host keeps the purse
    }

    [Fact]
    public void The_assassin_is_told_what_became_of_each_dagger()
    {
        var m = Scenario(assassinSeat: 0);
        Give(m, 2, ItemType.Amulet);
        Deliver(m, from: 0, to: 2, ItemType.Dagger); // stopped by an amulet
        Deliver(m, from: 0, to: 3, ItemType.Dagger); // kills
        GiftsGame.Dawn(m);

        var notes = m.Notes.Where(n => n.Seat == 0).Select(n => n.Text).ToList();
        Assert.Contains(notes, t => t.Contains(m.Players[2].Name) && t.Contains("amulet"));
        Assert.Contains(notes, t => t.Contains(m.Players[3].Name) && t.Contains("candle goes out"));
    }

    [Fact]
    public void The_host_sells_only_one_emergency_amulet_per_evening()
    {
        var m = Scenario();
        m.Phase = GiftsPhase.LastWhisper;
        var a = GiftsGame.AddOffer(m, m.Players[2], OfferKind.Amulet, 5, "now", emergency: true);
        var b = GiftsGame.AddOffer(m, m.Players[3], OfferKind.Amulet, 5, "now", emergency: true);
        m.Players[3].Gold = 99;
        m.Players[3].Traits = new Engine.Traits(FearOfLoss: 5, Greed: 1); // would buy at any price
        GiftsGame.AcceptOffer(m, 2, a.Id); // continues the evening: the NPC at seat 3 answers its own offer

        Assert.True(b.Resolved);
        Assert.True(m.Players[2].Has(ItemType.Amulet));
        Assert.False(m.Players[3].Has(ItemType.Amulet));
        Assert.Equal(1, m.EmergencyAmuletsSold);
    }

    [Fact]
    public void A_guest_dagger_is_harmless()
    {
        var m = Scenario();
        Deliver(m, from: 3, to: 2, ItemType.Dagger);
        GiftsGame.Dawn(m);
        Assert.True(m.Players[2].Alive);
        Assert.Equal(0, m.Deaths);
    }

    [Fact]
    public void The_first_candle_is_safe()
    {
        var m = Scenario(candle: 1);
        Deliver(m, from: 1, to: 2, ItemType.Dagger);
        GiftsGame.Dawn(m);
        Assert.True(m.Players[2].Alive);
    }

    [Fact]
    public void An_amulet_stops_one_death_and_both_leave_the_game()
    {
        var m = Scenario();
        Give(m, 2, ItemType.Amulet);
        Deliver(m, from: 1, to: 2, ItemType.Dagger);
        GiftsGame.Dawn(m);

        Assert.True(m.Players[2].Alive);
        Assert.False(m.Players[2].Has(ItemType.Amulet));
        Assert.False(m.Players[2].Has(ItemType.Dagger));
        Assert.Contains(m.Events, e => e.Kind == "amulet");
        Assert.DoesNotContain(m.Events, e => e.Kind == "amulet" && e.Text.Contains(m.Players[2].Name)); // nobody learns whose
    }

    [Fact]
    public void A_closed_door_sends_the_gift_back_unopened()
    {
        var m = Scenario();
        Give(m, 2, ItemType.ClosedDoor);
        var g = Deliver(m, from: 1, to: 2, ItemType.Dagger, DeliveryResponse.CloseDoor);
        GiftsGame.Dawn(m);

        Assert.Equal(GiftOutcome.DoorClosed, g.Outcome);
        Assert.True(m.Players[2].Alive);
        Assert.Contains(m.Players[1].Hand, i => i.Id == g.Card!.Id); // back with the Assassin
        Assert.False(m.Players[2].Has(ItemType.ClosedDoor));
    }

    [Fact]
    public void A_mirror_turns_the_assassins_dagger_into_his_own_heart()
    {
        var m = Scenario();
        Give(m, 2, ItemType.Mirror);
        var goldBefore = m.Players[2].Gold;
        Deliver(m, from: 1, to: 2, ItemType.Dagger, DeliveryResponse.Mirror);
        GiftsGame.Dawn(m);

        Assert.False(m.Players[1].Alive);
        Assert.True(m.Players[2].Alive);
        Assert.Equal(GiftsPhase.Finished, m.Phase);
        Assert.Equal(Winner.Guests, m.Winner);
        Assert.Equal(2, m.SlayerSeat);
        Assert.Equal(goldBefore + GiftsRules.AssassinSlayerBonus, m.Players[2].Gold);
    }

    [Fact]
    public void Poison_kills_at_the_next_dawn_not_this_one()
    {
        var m = Scenario();
        Deliver(m, from: 3, to: 2, ItemType.Poison);
        GiftsGame.Dawn(m);
        Assert.True(m.Players[2].Alive);

        m.Candle++;
        m.Phase = GiftsPhase.Delivery;
        GiftsGame.Dawn(m);
        Assert.False(m.Players[2].Alive);
        Assert.Equal(DeathCause.Poison, m.Players[2].DeathCause);
    }

    [Fact]
    public void An_amulet_bought_before_the_next_dawn_saves_the_poisoned()
    {
        var m = Scenario();
        Deliver(m, from: 3, to: 2, ItemType.Poison);
        GiftsGame.Dawn(m);
        m.Candle++;
        Give(m, 2, ItemType.Amulet);
        GiftsGame.Dawn(m);
        Assert.True(m.Players[2].Alive);
    }

    [Fact]
    public void Every_death_counts_towards_the_quota_and_the_assassin_wins_on_it()
    {
        var m = Scenario();
        Deliver(m, from: 3, to: 2, ItemType.Poison); // a guest poisons a guest…
        GiftsGame.Dawn(m);
        m.Candle++;
        Deliver(m, from: 1, to: 4, ItemType.Dagger); // …and the Assassin kills one
        GiftsGame.Dawn(m);

        Assert.Equal(2, m.Deaths);
        Assert.Equal(Winner.Assassin, m.Winner);
        Assert.Equal([1], m.WinnerSeats);
    }

    [Fact]
    public void Poisoning_the_assassin_ends_the_evening_for_the_guests()
    {
        var m = Scenario();
        Deliver(m, from: 3, to: 1, ItemType.Poison);
        GiftsGame.Dawn(m);
        m.Candle++;
        GiftsGame.Dawn(m);
        Assert.Equal(Winner.Guests, m.Winner);
        Assert.Equal(3, m.SlayerSeat);
    }

    [Fact]
    public void Daggers_cannot_be_discarded_while_other_cards_can()
    {
        var m = Scenario();
        m.Phase = GiftsPhase.Discard;
        var dagger = Give(m, 0, ItemType.Dagger);
        for (var i = 0; i < 5; i++) Give(m, 0, ItemType.Bread);

        var ex = Assert.Throws<GiftsRuleException>(() => GiftsGame.DiscardCards(m, 0, [dagger.Id]));
        Assert.Contains("Daggers", ex.Message);
    }

    [Fact]
    public void Gifts_must_go_to_someone_else_and_a_purse_cannot_exceed_your_gold()
    {
        var m = HumanMatch();
        GiftsGame.EndWhisper(m, 0);
        Assert.Equal(GiftsPhase.Gifts, m.Phase);

        Assert.Throws<GiftsRuleException>(() => GiftsGame.SendGift(m, 0, GiftKind.FreeBread, 0, Declaration.Bread));
        Assert.Throws<GiftsRuleException>(() => GiftsGame.SendGift(m, 0, GiftKind.Purse, 1, Declaration.Purse, purse: 999));
    }

    [Fact]
    public void The_view_hides_other_roles_hands_and_true_gold()
    {
        var m = HumanMatch();
        m.Players[2].Gold = 3; // a secret payment; public gold still says 12
        var v = GiftsView.ForSeat(m, 0);

        Assert.All(v.Players.Where(p => !p.IsMe), p =>
        {
            Assert.Null(p.Role);
            Assert.Null(p.RevealedHand);
        });
        Assert.Equal(12, v.Players[2].Gold);
        Assert.NotNull(v.Players[0].Role);
        Assert.Equal(m.Players[0].Hand.Count, v.Me.Hand.Count);
    }

    [Fact]
    public void Wine_reveals_the_sender_of_any_gift_tonight()
    {
        var m = HumanMatch(7);
        GiftsGame.EndWhisper(m, 0);
        var wine = GiftsGame.NewItem(m, ItemType.Wine);
        m.Players[1].Hand.Add(wine);
        // play the evening until the human must choose wine (if they got some) — simulated directly:
        var p = m.Players[0];
        p.PendingWine = 1;
        var g = new Gift { Id = m.Gifts.Count + 1, Candle = m.Candle, From = 3, To = 2, Kind = GiftKind.FreeBread, Declared = Declaration.Bread, Announced = true, Delivered = true };
        m.Gifts.Add(g);
        m.Phase = GiftsPhase.WineChoice;

        GiftsGame.ChooseWine(m, 0, g.Id);
        Assert.Contains(m.Notes, n => n.Seat == 0 && n.Text.Contains(m.Players[3].Name) && n.Text.Contains("wine"));
    }
}
