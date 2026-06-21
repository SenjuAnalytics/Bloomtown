using Bloomtown.Shared.Community;
using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Tests;

public sealed class VillageAreaConfigTests
{
    [Fact]
    public void All_DefinesThreeUnlockableAreas()
    {
        Assert.Equal(3, VillageAreaConfig.All.Count);
    }

    [Fact]
    public void AllInteractions_DefinesTwoPerArea()
    {
        Assert.Equal(6, VillageAreaConfig.AllInteractions.Count);
        Assert.Equal(2, VillageAreaConfig.GetInteractionsForArea(VillageArea.MarketSquare).Count);
        Assert.Equal(2, VillageAreaConfig.GetInteractionsForArea(VillageArea.CommunityGarden).Count);
        Assert.Equal(2, VillageAreaConfig.GetInteractionsForArea(VillageArea.RiversideWalk).Count);
    }

    [Theory]
    [InlineData(VillageArea.MarketSquare, VillageDevelopmentLevel.Lively)]
    [InlineData(VillageArea.CommunityGarden, VillageDevelopmentLevel.Bustling)]
    [InlineData(VillageArea.RiversideWalk, VillageDevelopmentLevel.Bustling)]
    public void IsUnlockedByLevel_UsesRequiredDevelopmentLevel(VillageArea area, VillageDevelopmentLevel required)
    {
        Assert.False(VillageAreaConfig.IsUnlockedByLevel(area, required - 1));
        Assert.True(VillageAreaConfig.IsUnlockedByLevel(area, required));
    }

    [Fact]
    public void TryGetInteraction_MapsBuiltinCommands()
    {
        Assert.True(VillageAreaConfig.TryGetInteraction(VillageAreaInteractionKind.BrowseMarket, out var market));
        Assert.Equal(VillageArea.MarketSquare, market.Area);
        Assert.Equal("browse market", market.CommandHint);

        Assert.True(VillageAreaConfig.TryGetInteraction(VillageAreaInteractionKind.ChatLocals, out var chat));
        Assert.Equal(VillageArea.MarketSquare, chat.Area);
        Assert.Equal("chat locals", chat.CommandHint);

        Assert.True(VillageAreaConfig.TryGetInteraction(VillageAreaInteractionKind.RelaxGarden, out var garden));
        Assert.Equal(VillageArea.CommunityGarden, garden.Area);

        Assert.True(VillageAreaConfig.TryGetInteraction(VillageAreaInteractionKind.TendPlants, out var tend));
        Assert.Equal(VillageArea.CommunityGarden, tend.Area);

        Assert.True(VillageAreaConfig.TryGetInteraction(VillageAreaInteractionKind.StrollRiver, out var river));
        Assert.Equal(VillageArea.RiversideWalk, river.Area);

        Assert.True(VillageAreaConfig.TryGetInteraction(VillageAreaInteractionKind.ReflectRiver, out var reflect));
        Assert.Equal(VillageArea.RiversideWalk, reflect.Area);
    }

    [Fact]
    public void FormatStatusLine_ShowsLockedAndUnlockedForms()
    {
        var definition = VillageAreaConfig.All[0];
        Assert.Contains("Locked", VillageAreaConfig.FormatStatusLine(definition, unlocked: false));
        Assert.Contains("Unlocked", VillageAreaConfig.FormatStatusLine(definition, unlocked: true));
        Assert.Contains("browse market", VillageAreaConfig.FormatStatusLine(definition, unlocked: true));
        Assert.Contains("chat locals", VillageAreaConfig.FormatStatusLine(definition, unlocked: true));
    }

    [Fact]
    public void FormatPassiveSummary_DescribesAreaBenefits()
    {
        var market = VillageAreaConfig.All.First(a => a.Area == VillageArea.MarketSquare);
        var garden = VillageAreaConfig.All.First(a => a.Area == VillageArea.CommunityGarden);
        var river = VillageAreaConfig.All.First(a => a.Area == VillageArea.RiversideWalk);

        Assert.Contains("Mood", VillageAreaConfig.FormatPassiveSummary(market));
        Assert.Contains("Fatigue", VillageAreaConfig.FormatPassiveSummary(garden));
        Assert.Contains("Mood", VillageAreaConfig.FormatPassiveSummary(river));
        Assert.Contains("Fatigue", VillageAreaConfig.FormatPassiveSummary(river));
    }

    [Fact]
    public void GetUnlockFlavor_ReturnsAnnouncementForEachArea()
    {
        foreach (var definition in VillageAreaConfig.All)
        {
            var flavor = VillageAreaConfig.GetUnlockFlavor(definition.Area);
            Assert.False(string.IsNullOrWhiteSpace(flavor));
        }
    }

    [Theory]
    [InlineData(VillageAreaInteractionKind.BrowseMarket)]
    [InlineData(VillageAreaInteractionKind.ChatLocals)]
    [InlineData(VillageAreaInteractionKind.RelaxGarden)]
    [InlineData(VillageAreaInteractionKind.TendPlants)]
    [InlineData(VillageAreaInteractionKind.StrollRiver)]
    [InlineData(VillageAreaInteractionKind.ReflectRiver)]
    public void GetInteractionCooldown_ReturnsPositiveDuration(VillageAreaInteractionKind interaction)
    {
        Assert.True(VillageAreaConfig.GetInteractionCooldown(interaction) > TimeSpan.Zero);
    }

    [Fact]
    public void PickInteractionFlavor_RotatesThroughLines()
    {
        var interaction = VillageAreaConfig.AllInteractions.First(i => i.Kind == VillageAreaInteractionKind.BrowseMarket);
        var first = VillageAreaConfig.PickInteractionFlavor(interaction, 0);
        var second = VillageAreaConfig.PickInteractionFlavor(interaction, 1);
        Assert.False(string.IsNullOrWhiteSpace(first));
        Assert.False(string.IsNullOrWhiteSpace(second));
        Assert.NotEqual(first, second);
    }
}