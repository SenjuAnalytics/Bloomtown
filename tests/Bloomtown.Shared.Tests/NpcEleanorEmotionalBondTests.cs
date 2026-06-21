using Bloomtown.Shared.Community;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Tests;

public sealed class NpcEleanorEmotionalBondTests
{
    [Fact]
    public void GetCompanionMemoryForNpc_ReturnsFrequentEleanorCompanion()
    {
        Assert.Equal(
            NpcMemoryType.FrequentEleanorCompanion,
            NpcEmotionalBondConfig.GetCompanionMemoryForNpc(NpcEntityIds.Eleanor));
    }

    [Fact]
    public void TryGetEmotionalInteractionResponse_ReturnsPorchLineForStoryMemory()
    {
        var memories = new[] { NpcMemoryType.ListenedToEleanorStories };

        var line = NpcEmotionalBondConfig.TryGetEmotionalInteractionResponse(
            NpcEntityIds.Eleanor,
            NpcInteractionKind.Talk,
            memories,
            RelationshipTier.Friend,
            LegacyArchetype.Connector,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("Bloomtown", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetArchetypeEmotionalBondLine_LinksConnectorToEleanor()
    {
        var line = NpcEmotionalBondConfig.TryGetArchetypeEmotionalBondLine(
            NpcEntityIds.Eleanor,
            LegacyArchetype.Connector,
            RelationshipTier.CloseFriend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("connector", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatEmotionalBondHint_ShowsStoryMemoryForEleanor()
    {
        var memories = new[] { NpcMemoryType.ListenedToEleanorStories };
        var hint = NpcEmotionalBondConfig.FormatEmotionalBondHint(NpcEntityIds.Eleanor, memories);

        Assert.NotNull(hint);
        Assert.Contains("porch", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void QualifiesForImpact_WorksForEleanorWithCompanionMemory()
    {
        var memories = new[] { NpcMemoryType.FrequentEleanorCompanion };

        Assert.True(NpcEmotionalBondImpactConfig.QualifiesForImpact(
            NpcEntityIds.Eleanor,
            RelationshipTier.Friend,
            memories));
    }

    [Fact]
    public void TryGetPersonalAppreciationLine_ReturnsEleanorLine()
    {
        var memories = new[] { NpcMemoryType.FrequentEleanorCompanion };

        var line = NpcEmotionalBondImpactConfig.TryGetPersonalAppreciationLine(
            NpcEntityIds.Eleanor,
            memories,
            RelationshipTier.Friend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("Eleanor", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetFavorItemGrant_ReturnsAppleForStoryListening()
    {
        var grant = NpcEmotionalBondImpactConfig.TryGetFavorItemGrant(
            NpcEntityIds.Eleanor,
            NpcMemoryType.ListenedToEleanorStories,
            RelationshipTier.Friend,
            bondingAction: null,
            variationSeed: 28);

        Assert.NotNull(grant);
        Assert.Equal(ItemType.Apple, grant.Value.ItemType);
    }

    [Fact]
    public void GetAreaHelpMemoryForActivity_MapsChatWithEleanorToEleanor()
    {
        Assert.Equal(
            NpcMemoryType.ListenedToEleanorStories,
            NpcEmotionalBondConfig.GetAreaHelpMemoryForActivity(CommunityActivityKind.ChatWithEleanor));
        Assert.Equal(
            NpcEntityIds.Eleanor,
            NpcEmotionalBondConfig.GetFocusNpcForActivity(CommunityActivityKind.ChatWithEleanor));
    }

    [Fact]
    public void GetMemoryForAction_MapsEleanorBondingActions()
    {
        Assert.Equal(
            NpcMemoryType.CheckedOnEleanor,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Eleanor, EmotionalBondActionKind.CheckOn));
        Assert.Equal(
            NpcMemoryType.SpentQuietTimeWithEleanor,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Eleanor, EmotionalBondActionKind.SpendTime));
        Assert.Equal(
            NpcMemoryType.ConsciouslyHelpedEleanor,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Eleanor, EmotionalBondActionKind.HelpWith));
    }

    [Fact]
    public void ResolveOutcome_EleanorReturnsLegacyOutcomes()
    {
        var outcome = SocialInfluenceActionConfig.ResolveOutcome(
            playerEntityId: 1,
            NpcEntityIds.Eleanor,
            variationSeed: 0);

        Assert.True(outcome is SocialInfluenceOutcomeKind.Info
            or SocialInfluenceOutcomeKind.LegacyBacking
            or SocialInfluenceOutcomeKind.Item);
    }

    [Fact]
    public void IsSupportedNpc_IncludesEleanor()
    {
        Assert.True(SocialInfluenceActionConfig.IsSupportedNpc(NpcEntityIds.Eleanor));
        Assert.Contains(NpcEntityIds.Eleanor, SocialInfluenceActionConfig.SupportedNpcEntityIds);
    }

    [Fact]
    public void GetInteractionBonus_EleanorAddsWarmNostalgicExtra()
    {
        var baseTalk = VillageSocialStandingImpactConfig.GetInteractionBonus(
            VillageSocialStandingTier.WellLiked,
            NpcEntityIds.Mira);
        var eleanorTalk = VillageSocialStandingImpactConfig.GetInteractionBonus(
            VillageSocialStandingTier.WellLiked,
            NpcEntityIds.Eleanor);

        Assert.True(eleanorTalk.MoodBonus > baseTalk.MoodBonus);
        Assert.True(eleanorTalk.SocialBonus > baseTalk.SocialBonus);
    }

    [Fact]
    public void IsFocusNpc_IncludesEleanorAsTwelfthFocusNpc()
    {
        Assert.True(NpcEmotionalBondConfig.IsFocusNpc(NpcEntityIds.Eleanor));
        Assert.Equal(12, NpcEmotionalBondConfig.FocusNpcEntityIds.Length);
        Assert.Equal(NpcEntityIds.Eleanor, NpcEmotionalBondConfig.FocusNpcEntityIds[11]);
    }
}