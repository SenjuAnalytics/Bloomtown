using Bloomtown.Shared.Community;
using Bloomtown.Shared.Npc;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Tests;

public sealed class VillageSocialCircleConfigTests
{
    [Fact]
    public void GetPairRelationship_ReturnsStaticTones()
    {
        Assert.Equal(
            NpcInterpersonalRelationship.Friendly,
            VillageSocialCircleConfig.GetPairRelationship(VillageSocialPair.ElsieMira));
        Assert.Equal(
            NpcInterpersonalRelationship.Neutral,
            VillageSocialCircleConfig.GetPairRelationship(VillageSocialPair.TomHarold));
    }

    [Fact]
    public void TryGetExpandedNpcToNpcComment_ReturnsMiraOrHaroldLine()
    {
        var comment = VillageSocialCircleConfig.TryGetExpandedNpcToNpcComment(
            GameTimeOfDay.Morning,
            VillageDevelopmentLevel.Lively,
            NpcInterpersonalRelationship.Friendly,
            variationSeed: 0,
            out var speakerId,
            out var socialPair);

        Assert.False(string.IsNullOrWhiteSpace(comment));
        Assert.NotNull(socialPair);
        Assert.True(VillageSocialCircleConfig.IsSocialCircleVillager(speakerId));
        Assert.True(
            comment.Contains("Mira", StringComparison.OrdinalIgnoreCase)
            || comment.Contains("Harold", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TryGetExpandedNpcToNpcComment_ElsieMiraDiffersFromTomHarold()
    {
        var elsieMira = VillageSocialCircleConfig.TryGetExpandedNpcToNpcComment(
            GameTimeOfDay.Afternoon,
            VillageDevelopmentLevel.Lively,
            NpcInterpersonalRelationship.Neutral,
            variationSeed: 0,
            out _,
            out var pairA);

        var tomHarold = VillageSocialCircleConfig.TryGetExpandedNpcToNpcComment(
            GameTimeOfDay.Afternoon,
            VillageDevelopmentLevel.Lively,
            NpcInterpersonalRelationship.Neutral,
            variationSeed: 1,
            out _,
            out var pairB);

        Assert.Equal(VillageSocialPair.ElsieMira, pairA);
        Assert.Equal(VillageSocialPair.TomHarold, pairB);
        Assert.False(string.IsNullOrWhiteSpace(elsieMira));
        Assert.False(string.IsNullOrWhiteSpace(tomHarold));
        Assert.NotEqual(elsieMira, tomHarold);
    }

    [Fact]
    public void TryGetSocialCircleGossip_ReturnsWiderCircleFlavor()
    {
        var gossip = VillageSocialCircleConfig.TryGetSocialCircleGossip(
            GameTimeOfDay.Evening,
            VillageDevelopmentLevel.Lively,
            NpcInterpersonalRelationship.Friendly,
            variationSeed: 0,
            out var kind,
            out _);

        Assert.False(string.IsNullOrWhiteSpace(gossip));
        Assert.Equal(VillageGossipKind.WiderSocialCircle, kind);
        Assert.True(
            gossip.Contains("Mira", StringComparison.OrdinalIgnoreCase)
            || gossip.Contains("Harold", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void TryGetGroupSocialMoment_InvolvesMultipleNpcs()
    {
        var moment = VillageSocialCircleConfig.TryGetGroupSocialMoment(
            GameTimeOfDay.Afternoon,
            VillageDevelopmentLevel.Bustling,
            NpcInterpersonalRelationship.Friendly,
            variationSeed: 1,
            out var kind);

        Assert.False(string.IsNullOrWhiteSpace(moment));
        Assert.Equal(VillageCommunityMomentKind.GroupGathering, kind);
        Assert.Contains("Mira", moment, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Harold", moment, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetNpcToNpcComment_CanReturnExpandedCircleLine()
    {
        var comment = VillageCommunityLifeConfig.TryGetNpcToNpcComment(
            GameTimeOfDay.Morning,
            VillageDevelopmentLevel.Quiet,
            Array.Empty<byte>(),
            NpcInterpersonalRelationship.Friendly,
            variationSeed: 6,
            out var speakerId,
            out var socialPair);

        Assert.False(string.IsNullOrWhiteSpace(comment));
        Assert.NotNull(socialPair);
        Assert.True(VillageCommunityLifeConfig.IsKnownVillager(speakerId));
    }

    [Fact]
    public void IsSocialCircleVillager_IncludesMiraAndHarold()
    {
        Assert.True(VillageSocialCircleConfig.IsSocialCircleVillager(NpcEntityIds.Mira));
        Assert.True(VillageSocialCircleConfig.IsSocialCircleVillager(NpcEntityIds.Harold));
    }

    [Fact]
    public void FormatSocialCircleStatus_MentionsNewNeighbors()
    {
        var status = VillageSocialCircleConfig.FormatSocialCircleStatus();

        Assert.Contains("Mira", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Harold", status, StringComparison.OrdinalIgnoreCase);
    }
}