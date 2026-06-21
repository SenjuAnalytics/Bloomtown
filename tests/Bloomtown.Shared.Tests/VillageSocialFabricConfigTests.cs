using Bloomtown.Shared.Community;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Legacy;
using Bloomtown.Shared.Npc;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Tests;

public sealed class VillageSocialFabricConfigTests
{
    [Fact]
    public void TryGetCommunityMoment_ReturnsFlavorLine()
    {
        var moment = VillageSocialFabricConfig.TryGetCommunityMoment(
            GameTimeOfDay.Afternoon,
            VillageDevelopmentLevel.Lively,
            NpcInterpersonalRelationship.Friendly,
            variationSeed: 0,
            out var kind);

        Assert.False(string.IsNullOrWhiteSpace(moment));
        Assert.True(Enum.IsDefined(kind));
    }

    [Theory]
    [InlineData(CommunityActivityKind.HelpGarden, NpcEntityIds.Elsie)]
    [InlineData(CommunityActivityKind.HelpMarket, NpcEntityIds.Tom)]
    [InlineData(CommunityActivityKind.HelpWell, NpcEntityIds.Elsie)]
    public void TryGetCommunityHelpAmbientReaction_ReturnsPersonalizedLine(
        CommunityActivityKind activity,
        uint npcEntityId)
    {
        var reaction = VillageSocialFabricConfig.TryGetCommunityHelpAmbientReaction(
            activity,
            NpcInterpersonalRelationship.Friendly,
            npcEntityId,
            variationSeed: 1);

        Assert.False(string.IsNullOrWhiteSpace(reaction));
    }

    [Theory]
    [InlineData(CommunityActivityKind.HelpGarden)]
    [InlineData(CommunityActivityKind.HelpMarket)]
    [InlineData(CommunityActivityKind.HelpWell)]
    public void TryGetCommunityHelpFollowUp_ReturnsNonEmptyLine(CommunityActivityKind activity)
    {
        var followUp = VillageSocialFabricConfig.TryGetCommunityHelpFollowUp(
            activity,
            NpcInterpersonalRelationship.Friendly,
            variationSeed: 2);

        Assert.False(string.IsNullOrWhiteSpace(followUp));
    }

    [Fact]
    public void ShouldTriggerCommunityMoment_UsesVeryLowThreshold()
    {
        const uint attempt = 1;
        var roll = (5u * 59 + (uint)(100 % 977) + attempt * 11) % 100;
        Assert.Equal(roll < VillageSocialFabricConfig.CommunityMomentChancePercent,
            VillageSocialFabricConfig.ShouldTriggerCommunityMoment(5, 100, attempt));
    }

    [Fact]
    public void FormatSocialFabricStatus_IncludesBelongingForHelpers()
    {
        var context = new PlayerLegacyContext
        {
            Markers = PlayerLegacyMarker.HelpedCommunityProject,
            VillageTitle = VillageTitle.Helper,
            VillageContributionScore = 10,
        };

        var status = VillageSocialFabricConfig.FormatSocialFabricStatus(
            GameTimeOfDay.Morning,
            VillageDevelopmentLevel.Lively,
            context);

        Assert.Contains("social life", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("lend a hand", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetVillageGossip_ReturnsInterpersonalProjectGossip()
    {
        var completed = new[] { VillageProjectBenefitConfig.WellProjectId };
        var gossip = VillageCommunityLifeConfig.TryGetVillageGossip(
            GameTimeOfDay.Morning,
            VillageDevelopmentLevel.Lively,
            completed,
            NpcInterpersonalRelationship.Friendly,
            variationSeed: 4,
            out var kind,
            out _);

        Assert.False(string.IsNullOrWhiteSpace(gossip));
        Assert.Equal(VillageGossipKind.SocialConnection, kind);
        Assert.True(
            gossip.Contains("Elsie", StringComparison.OrdinalIgnoreCase)
            || gossip.Contains("Tom", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TryGetNpcToNpcComment_CanReturnBondLineForFriendlyRelationship()
    {
        var comment = VillageCommunityLifeConfig.TryGetNpcToNpcComment(
            GameTimeOfDay.Morning,
            VillageDevelopmentLevel.Lively,
            Array.Empty<byte>(),
            NpcInterpersonalRelationship.Friendly,
            variationSeed: 5,
            out var speakerId,
            out _);

        Assert.False(string.IsNullOrWhiteSpace(comment));
        Assert.True(VillageCommunityLifeConfig.IsKnownVillager(speakerId));
    }
}