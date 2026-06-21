using Bloomtown.Shared.Community;
using Bloomtown.Shared.Npc;
using Bloomtown.Shared.Village;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Tests;

public sealed class NpcInterpersonalRelationshipConfigTests
{
    [Fact]
    public void ResolveRelationship_StaysNeutralWithoutTriggers()
    {
        var result = NpcInterpersonalRelationshipConfig.ResolveRelationship(
            NpcInterpersonalRelationship.Neutral,
            Array.Empty<byte>(),
            gameDay: 3);

        Assert.Equal(NpcInterpersonalRelationship.Neutral, result);
    }

    [Fact]
    public void ResolveRelationship_BecomesFriendlyAfterWellCompletion()
    {
        var completed = new[] { VillageProjectBenefitConfig.WellProjectId };

        var result = NpcInterpersonalRelationshipConfig.ResolveRelationship(
            NpcInterpersonalRelationship.Neutral,
            completed,
            gameDay: 1);

        Assert.Equal(NpcInterpersonalRelationship.Friendly, result);
    }

    [Fact]
    public void ResolveRelationship_BecomesFriendlyAfterBridgeCompletion()
    {
        var completed = new[] { VillageProjectBenefitConfig.BridgeProjectId };

        var result = NpcInterpersonalRelationshipConfig.ResolveRelationship(
            NpcInterpersonalRelationship.Neutral,
            completed,
            gameDay: 1);

        Assert.Equal(NpcInterpersonalRelationship.Friendly, result);
    }

    [Fact]
    public void ResolveRelationship_BecomesFriendlyAfterWarehouseCompletion()
    {
        var completed = new[] { VillageProjectBenefitConfig.WarehouseProjectId };

        var result = NpcInterpersonalRelationshipConfig.ResolveRelationship(
            NpcInterpersonalRelationship.Neutral,
            completed,
            gameDay: 1);

        Assert.Equal(NpcInterpersonalRelationship.Friendly, result);
    }

    [Fact]
    public void ResolveRelationship_TimeEvolutionRequiresCompletedProject()
    {
        var result = NpcInterpersonalRelationshipConfig.ResolveRelationship(
            NpcInterpersonalRelationship.Neutral,
            Array.Empty<byte>(),
            gameDay: NpcInterpersonalRelationshipConfig.TimeEvolutionMinGameDay);

        Assert.Equal(NpcInterpersonalRelationship.Neutral, result);
    }

    [Fact]
    public void ResolveRelationship_TimeEvolutionWarmsAfterEnoughDays()
    {
        var completed = new[] { VillageProjectBenefitConfig.WarehouseProjectId };

        var result = NpcInterpersonalRelationshipConfig.ResolveRelationship(
            NpcInterpersonalRelationship.Neutral,
            completed,
            gameDay: NpcInterpersonalRelationshipConfig.TimeEvolutionMinGameDay);

        Assert.Equal(NpcInterpersonalRelationship.Friendly, result);
    }

    [Fact]
    public void TryGetNpcToNpcComment_UsesWarmerToneWhenFriendly()
    {
        var friendly = VillageCommunityLifeConfig.TryGetNpcToNpcComment(
            GameTimeOfDay.Morning,
            VillageDevelopmentLevel.Quiet,
            Array.Empty<byte>(),
            NpcInterpersonalRelationship.Friendly,
            variationSeed: 0,
            out _,
            out _);

        var neutral = VillageCommunityLifeConfig.TryGetNpcToNpcComment(
            GameTimeOfDay.Morning,
            VillageDevelopmentLevel.Quiet,
            Array.Empty<byte>(),
            NpcInterpersonalRelationship.Neutral,
            variationSeed: 0,
            out _,
            out _);

        Assert.False(string.IsNullOrWhiteSpace(friendly));
        Assert.False(string.IsNullOrWhiteSpace(neutral));
        Assert.NotEqual(friendly, neutral);
    }

    [Fact]
    public void TryGetNpcToNpcComment_ProjectLinesDifferByRelationship()
    {
        var completed = new[] { VillageProjectBenefitConfig.WellProjectId };

        var friendly = VillageCommunityLifeConfig.TryGetNpcToNpcComment(
            GameTimeOfDay.Afternoon,
            VillageDevelopmentLevel.Lively,
            completed,
            NpcInterpersonalRelationship.Friendly,
            variationSeed: 1,
            out _,
            out _);

        var neutral = VillageCommunityLifeConfig.TryGetNpcToNpcComment(
            GameTimeOfDay.Afternoon,
            VillageDevelopmentLevel.Lively,
            completed,
            NpcInterpersonalRelationship.Neutral,
            variationSeed: 1,
            out _,
            out _);

        Assert.False(string.IsNullOrWhiteSpace(friendly));
        Assert.False(string.IsNullOrWhiteSpace(neutral));
        Assert.NotEqual(friendly, neutral);
    }

    [Fact]
    public void FormatRelationshipStatus_DiffersByTone()
    {
        var friendly = NpcInterpersonalRelationshipConfig.FormatRelationshipStatus(
            NpcInterpersonalRelationship.Friendly);
        var neutral = NpcInterpersonalRelationshipConfig.FormatRelationshipStatus(
            NpcInterpersonalRelationship.Neutral);

        Assert.Contains("friendly", friendly, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("practical", neutral, StringComparison.OrdinalIgnoreCase);
        Assert.NotEqual(friendly, neutral);
    }

    [Fact]
    public void TryGetEmergentSocialMoment_ReflectsRelationshipTone()
    {
        var friendly = VillageCommunityLifeConfig.TryGetEmergentSocialMoment(
            GameTimeOfDay.Afternoon,
            VillageDevelopmentLevel.Lively,
            NpcInterpersonalRelationship.Friendly,
            variationSeed: 0,
            out _);

        var neutral = VillageCommunityLifeConfig.TryGetEmergentSocialMoment(
            GameTimeOfDay.Afternoon,
            VillageDevelopmentLevel.Lively,
            NpcInterpersonalRelationship.Neutral,
            variationSeed: 0,
            out _);

        Assert.False(string.IsNullOrWhiteSpace(friendly));
        Assert.False(string.IsNullOrWhiteSpace(neutral));
        Assert.NotEqual(friendly, neutral);
        Assert.True(
            friendly!.Contains("Elsie", StringComparison.OrdinalIgnoreCase)
            && friendly.Contains("Tom", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TryGetVillageGossip_SocialConnectionReflectsRelationship()
    {
        var friendly = VillageCommunityLifeConfig.TryGetVillageGossip(
            GameTimeOfDay.Evening,
            VillageDevelopmentLevel.Quiet,
            Array.Empty<byte>(),
            NpcInterpersonalRelationship.Friendly,
            variationSeed: 4,
            out var friendlyKind,
            out _);

        var neutral = VillageCommunityLifeConfig.TryGetVillageGossip(
            GameTimeOfDay.Evening,
            VillageDevelopmentLevel.Quiet,
            Array.Empty<byte>(),
            NpcInterpersonalRelationship.Neutral,
            variationSeed: 4,
            out var neutralKind,
            out _);

        Assert.Equal(VillageGossipKind.SocialConnection, friendlyKind);
        Assert.Equal(VillageGossipKind.SocialConnection, neutralKind);
        Assert.True(
            friendly!.Contains("Elsie", StringComparison.OrdinalIgnoreCase)
            && friendly.Contains("Tom", StringComparison.OrdinalIgnoreCase));
        Assert.True(
            neutral!.Contains("dependable", StringComparison.OrdinalIgnoreCase)
            || neutral.Contains("side by side", StringComparison.OrdinalIgnoreCase)
            || neutral.Contains("coordinate", StringComparison.OrdinalIgnoreCase));
    }
}