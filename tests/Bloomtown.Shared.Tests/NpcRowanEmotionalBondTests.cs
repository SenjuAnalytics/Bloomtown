using Bloomtown.Shared.Community;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Tests;

public sealed class NpcRowanEmotionalBondTests
{
    [Fact]
    public void GetCompanionMemoryForNpc_ReturnsFrequentRowanCompanion()
    {
        Assert.Equal(
            NpcMemoryType.FrequentRowanCompanion,
            NpcEmotionalBondConfig.GetCompanionMemoryForNpc(NpcEntityIds.Rowan));
    }

    [Fact]
    public void TryGetEmotionalInteractionResponse_ReturnsStoryLineForStoryMemory()
    {
        var memories = new[] { NpcMemoryType.ListenedToStoriesOften };

        var line = NpcEmotionalBondConfig.TryGetEmotionalInteractionResponse(
            NpcEntityIds.Rowan,
            NpcInteractionKind.Talk,
            memories,
            RelationshipTier.Friend,
            LegacyArchetype.Connector,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("story", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetArchetypeEmotionalBondLine_LinksConnectorToRowan()
    {
        var line = NpcEmotionalBondConfig.TryGetArchetypeEmotionalBondLine(
            NpcEntityIds.Rowan,
            LegacyArchetype.Connector,
            RelationshipTier.CloseFriend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("connector", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatEmotionalBondHint_ShowsStoryMemoryForRowan()
    {
        var memories = new[] { NpcMemoryType.ListenedToStoriesOften };
        var hint = NpcEmotionalBondConfig.FormatEmotionalBondHint(NpcEntityIds.Rowan, memories);

        Assert.NotNull(hint);
        Assert.Contains("story", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void QualifiesForImpact_WorksForRowanWithCompanionMemory()
    {
        var memories = new[] { NpcMemoryType.FrequentRowanCompanion };

        Assert.True(NpcEmotionalBondImpactConfig.QualifiesForImpact(
            NpcEntityIds.Rowan,
            RelationshipTier.Friend,
            memories));
    }

    [Fact]
    public void TryGetPersonalAppreciationLine_ReturnsRowanLine()
    {
        var memories = new[] { NpcMemoryType.FrequentRowanCompanion };

        var line = NpcEmotionalBondImpactConfig.TryGetPersonalAppreciationLine(
            NpcEntityIds.Rowan,
            memories,
            RelationshipTier.Friend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("Rowan", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetFavorItemGrant_ReturnsWoodForStoryListening()
    {
        var grant = NpcEmotionalBondImpactConfig.TryGetFavorItemGrant(
            NpcEntityIds.Rowan,
            NpcMemoryType.ListenedToStoriesOften,
            RelationshipTier.Friend,
            bondingAction: null,
            variationSeed: 0);

        Assert.NotNull(grant);
        Assert.Equal(ItemType.Wood, grant.Value.ItemType);
    }

    [Fact]
    public void GetAreaHelpMemoryForActivity_MapsListenToStoriesToRowan()
    {
        Assert.Equal(
            NpcMemoryType.ListenedToStoriesOften,
            NpcEmotionalBondConfig.GetAreaHelpMemoryForActivity(CommunityActivityKind.ListenToStories));
        Assert.Equal(
            NpcEntityIds.Rowan,
            NpcEmotionalBondConfig.GetFocusNpcForActivity(CommunityActivityKind.ListenToStories));
    }

    [Fact]
    public void GetMemoryForAction_MapsRowanBondingActions()
    {
        Assert.Equal(
            NpcMemoryType.CheckedOnRowan,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Rowan, EmotionalBondActionKind.CheckOn));
        Assert.Equal(
            NpcMemoryType.SpentQuietTimeWithRowan,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Rowan, EmotionalBondActionKind.SpendTime));
        Assert.Equal(
            NpcMemoryType.ConsciouslyHelpedRowan,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Rowan, EmotionalBondActionKind.HelpWith));
    }

    [Fact]
    public void ResolveOutcome_RowanReturnsStoryOutcomes()
    {
        var outcome = SocialInfluenceActionConfig.ResolveOutcome(
            playerEntityId: 1,
            NpcEntityIds.Rowan,
            variationSeed: 0);

        Assert.True(outcome is SocialInfluenceOutcomeKind.Info
            or SocialInfluenceOutcomeKind.StoryBacking
            or SocialInfluenceOutcomeKind.Item);
    }

    [Fact]
    public void IsSupportedNpc_IncludesRowan()
    {
        Assert.True(SocialInfluenceActionConfig.IsSupportedNpc(NpcEntityIds.Rowan));
        Assert.Contains(NpcEntityIds.Rowan, SocialInfluenceActionConfig.SupportedNpcEntityIds);
    }

    [Fact]
    public void GetInteractionRecoveryBonus_RowanAddsWiseWarmthExtra()
    {
        var baseTalk = NpcEmotionalBondImpactConfig.GetInteractionRecoveryBonus(
            NpcInteractionKind.Talk,
            RelationshipTier.Friend);
        var rowanTalk = NpcEmotionalBondImpactConfig.GetInteractionRecoveryBonus(
            NpcInteractionKind.Talk,
            RelationshipTier.Friend,
            NpcEntityIds.Rowan);

        Assert.True(rowanTalk.MoodBonus > baseTalk.MoodBonus);
        Assert.True(rowanTalk.SocialBonus > baseTalk.SocialBonus);
    }
}