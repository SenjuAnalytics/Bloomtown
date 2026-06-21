using Bloomtown.Shared.Community;
using Bloomtown.Shared.Npc;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Tests;

public sealed class VillageCommunityLifeConfigTests
{
    [Fact]
    public void TryGetNpcToNpcComment_ReturnsSocialLineWithPreferredSpeaker()
    {
        var comment = VillageCommunityLifeConfig.TryGetNpcToNpcComment(
            GameTimeOfDay.Morning,
            VillageDevelopmentLevel.Quiet,
            Array.Empty<byte>(),
            NpcInterpersonalRelationshipConfig.DefaultRelationship,
            variationSeed: 0,
            out var speakerId,
            out _);

        Assert.False(string.IsNullOrWhiteSpace(comment));
        Assert.True(VillageCommunityLifeConfig.IsKnownVillager(speakerId));
        Assert.True(
            comment.Contains("Tom", StringComparison.OrdinalIgnoreCase)
            || comment.Contains("Elsie", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TryGetNpcToNpcComment_PrefersProjectLinesWhenCompleted()
    {
        var completed = new[] { VillageProjectBenefitConfig.BridgeProjectId };
        var comment = VillageCommunityLifeConfig.TryGetNpcToNpcComment(
            GameTimeOfDay.Afternoon,
            VillageDevelopmentLevel.Lively,
            completed,
            NpcInterpersonalRelationship.Friendly,
            variationSeed: 1,
            out var speakerId,
            out _);

        Assert.False(string.IsNullOrWhiteSpace(comment));
        Assert.True(VillageCommunityLifeConfig.IsKnownVillager(speakerId));
        Assert.Contains("bridge", comment, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetVillageGossip_ReturnsProjectPrideWhenProjectsComplete()
    {
        var completed = new[] { VillageProjectBenefitConfig.WellProjectId };
        var gossip = VillageCommunityLifeConfig.TryGetVillageGossip(
            GameTimeOfDay.Morning,
            VillageDevelopmentLevel.Lively,
            completed,
            NpcInterpersonalRelationship.Friendly,
            variationSeed: 2,
            out var kind,
            out _);

        Assert.False(string.IsNullOrWhiteSpace(gossip));
        Assert.Equal(VillageGossipKind.ProjectPride, kind);
    }

    [Fact]
    public void TryGetVillageGossip_ReturnsNeighborlyRumorByDefault()
    {
        var gossip = VillageCommunityLifeConfig.TryGetVillageGossip(
            GameTimeOfDay.Evening,
            VillageDevelopmentLevel.Quiet,
            Array.Empty<byte>(),
            NpcInterpersonalRelationshipConfig.DefaultRelationship,
            variationSeed: 3,
            out var kind,
            out _);

        Assert.False(string.IsNullOrWhiteSpace(gossip));
        Assert.Equal(VillageGossipKind.NeighborlyRumor, kind);
        Assert.Contains("Elsie", gossip, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShouldTriggerNpcToNpcComment_UsesLowThreshold()
    {
        var roll = (5u * 43 + (uint)(100 % 991)) % 100;
        Assert.Equal(16, VillageCommunityLifeConfig.NpcToNpcChancePercent);
        Assert.Equal(roll < VillageCommunityLifeConfig.NpcToNpcChancePercent,
            VillageCommunityLifeConfig.ShouldTriggerNpcToNpcComment(5, 100));
    }

    [Fact]
    public void ShouldTriggerVillageGossip_UsesLowThreshold()
    {
        const uint attempt = 2;
        var roll = (5u * 47 + (uint)(100 % 983) + attempt * 13) % 100;
        Assert.Equal(10, VillageCommunityLifeConfig.VillageGossipChancePercent);
        Assert.Equal(roll < VillageCommunityLifeConfig.VillageGossipChancePercent,
            VillageCommunityLifeConfig.ShouldTriggerVillageGossip(5, 100, attempt));
    }

    [Fact]
    public void TryGetPrimaryCompletedProject_UsesStableOrder()
    {
        var completed = new[]
        {
            VillageProjectBenefitConfig.WarehouseProjectId,
            VillageProjectBenefitConfig.WellProjectId,
        };

        var primary = VillageCommunityLifeConfig.TryGetPrimaryCompletedProject(completed);

        Assert.Equal(VillageProjectBenefitConfig.WellProjectId, primary);
    }

    [Fact]
    public void VillageLifeDialogue_DelegatesNpcToNpcToCommunityLife()
    {
        var comment = VillageLifeDialogue.TryGetNpcToNpcComment(
            GameTimeOfDay.Morning,
            VillageDevelopmentLevel.Quiet,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(comment));
        Assert.True(
            comment.Contains("Tom", StringComparison.OrdinalIgnoreCase)
            || comment.Contains("Elsie", StringComparison.OrdinalIgnoreCase));
    }
}