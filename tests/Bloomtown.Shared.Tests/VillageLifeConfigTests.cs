using Bloomtown.Shared.Community;
using Bloomtown.Shared.Npc;
using Bloomtown.Shared.Village;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Tests;

public sealed class VillageLifeConfigTests
{
    [Theory]
    [InlineData(5, GameTimeOfDay.Morning)]
    [InlineData(11, GameTimeOfDay.Morning)]
    [InlineData(12, GameTimeOfDay.Afternoon)]
    [InlineData(16, GameTimeOfDay.Afternoon)]
    [InlineData(17, GameTimeOfDay.Evening)]
    [InlineData(20, GameTimeOfDay.Evening)]
    [InlineData(21, GameTimeOfDay.Night)]
    [InlineData(4, GameTimeOfDay.Night)]
    [InlineData(0, GameTimeOfDay.Night)]
    public void GetTimeOfDay_MapsGameHourToBand(int gameHour, GameTimeOfDay expected)
    {
        Assert.Equal(expected, VillageLifeConfig.GetTimeOfDay(gameHour));
    }

    [Fact]
    public void FormatGameTimeStatus_IncludesDayClockAndBand()
    {
        var status = VillageLifeConfig.FormatGameTimeStatus(3, 14, 32);
        Assert.Contains("Day 3", status);
        Assert.Contains("14:32", status);
        Assert.Contains("Afternoon", status);
    }

    [Theory]
    [InlineData(GameTimeOfDay.Morning, VillageDevelopmentLevel.Quiet)]
    [InlineData(GameTimeOfDay.Afternoon, VillageDevelopmentLevel.Lively)]
    [InlineData(GameTimeOfDay.Evening, VillageDevelopmentLevel.Bustling)]
    [InlineData(GameTimeOfDay.Night, VillageDevelopmentLevel.Quiet)]
    public void FormatVillageRhythm_ReturnsNonEmptyLine(GameTimeOfDay time, VillageDevelopmentLevel level)
    {
        var rhythm = VillageLifeConfig.FormatVillageRhythm(time, level);
        Assert.StartsWith("Village rhythm:", rhythm);
        Assert.False(string.IsNullOrWhiteSpace(rhythm));
    }

    [Fact]
    public void TryResolveAmbientLocation_PrefersNearestUnlockedArea()
    {
        var unlocked = new HashSet<VillageArea> { VillageArea.MarketSquare };
        var atMarket = VillageAreaConfig.All.First(a => a.Area == VillageArea.MarketSquare);

        Assert.True(VillageLifeConfig.TryResolveAmbientLocation(
            atMarket.WorldX,
            atMarket.WorldZ,
            unlocked,
            Array.Empty<byte>(),
            out var location,
            out _));

        Assert.Equal(VillageAmbientLocation.MarketSquare, location);
    }

    [Fact]
    public void TryResolveAmbientLocation_ResolvesCompletedWellSite()
    {
        Assert.True(VillageLifeConfig.TryResolveAmbientLocation(
            VillageSiteConfig.WellWorldX,
            VillageSiteConfig.WellWorldZ,
            new HashSet<VillageArea>(),
            new[] { VillageSiteIds.Well },
            out var location,
            out _));

        Assert.Equal(VillageAmbientLocation.VillageWell, location);
    }

    [Fact]
    public void TryResolveAmbientLocation_ReturnsGeneralWhenNowhereNear()
    {
        Assert.True(VillageLifeConfig.TryResolveAmbientLocation(
            999f,
            999f,
            new HashSet<VillageArea>(),
            Array.Empty<byte>(),
            out var location,
            out _));

        Assert.Equal(VillageAmbientLocation.General, location);
    }

    [Fact]
    public void ShouldTriggerEmergentEvent_IsDeterministicAndUsesLowThreshold()
    {
        const uint playerId = 5;
        const long totalMinutes = 100;
        const uint attempt = 2;

        var first = VillageLifeConfig.ShouldTriggerEmergentEvent(playerId, totalMinutes, attempt);
        var second = VillageLifeConfig.ShouldTriggerEmergentEvent(playerId, totalMinutes, attempt);
        Assert.Equal(first, second);

        var roll = (int)((playerId * 31 + (uint)(totalMinutes % 997) + attempt * 17) % 100);
        Assert.Equal(roll < VillageLifeConfig.EmergentEventChancePercent, first);
    }

    [Fact]
    public void VillageLifeDialogue_ReturnsLocationTimeComment()
    {
        var comment = VillageLifeDialogue.TryGetLocationTimeComment(
            VillageAmbientLocation.MarketSquare,
            GameTimeOfDay.Morning,
            VillageDevelopmentLevel.Lively,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(comment));
    }

    [Fact]
    public void VillageLifeDialogue_ReturnsEmergentEvent()
    {
        var comment = VillageLifeDialogue.TryGetEmergentEvent(
            GameTimeOfDay.Afternoon,
            VillageDevelopmentLevel.Lively,
            variationSeed: 2,
            out var kind);

        Assert.False(string.IsNullOrWhiteSpace(comment));
        Assert.Equal(VillageEmergentEventKind.VillageMoodShift, kind);
    }

    [Fact]
    public void VillageLifeDialogue_EmergentEventDiffersByRelationship()
    {
        var friendly = VillageLifeDialogue.TryGetEmergentEvent(
            GameTimeOfDay.Afternoon,
            VillageDevelopmentLevel.Lively,
            NpcInterpersonalRelationship.Friendly,
            variationSeed: 1,
            out _);

        var neutral = VillageLifeDialogue.TryGetEmergentEvent(
            GameTimeOfDay.Afternoon,
            VillageDevelopmentLevel.Lively,
            NpcInterpersonalRelationship.Neutral,
            variationSeed: 1,
            out _);

        Assert.False(string.IsNullOrWhiteSpace(friendly));
        Assert.False(string.IsNullOrWhiteSpace(neutral));
        Assert.NotEqual(friendly, neutral);
    }

    [Fact]
    public void VillageLifeDialogue_ReturnsNpcToNpcComment()
    {
        var comment = VillageLifeDialogue.TryGetNpcToNpcComment(
            GameTimeOfDay.Morning,
            VillageDevelopmentLevel.Quiet,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(comment));
        Assert.True(
            comment.Contains("Elsie", StringComparison.OrdinalIgnoreCase)
            || comment.Contains("Tom", StringComparison.OrdinalIgnoreCase));
    }
}