using Bloomtown.Shared.Community;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Tests;

public sealed class NpcLilaEmotionalBondTests
{
    [Fact]
    public void GetCompanionMemoryForNpc_ReturnsFrequentLilaCompanion()
    {
        Assert.Equal(
            NpcMemoryType.FrequentLilaCompanion,
            NpcEmotionalBondConfig.GetCompanionMemoryForNpc(NpcEntityIds.Lila));
    }

    [Fact]
    public void TryGetEmotionalInteractionResponse_ReturnsVillageLineForVillageMemory()
    {
        var memories = new[] { NpcMemoryType.HelpedVillageOften };

        var line = NpcEmotionalBondConfig.TryGetEmotionalInteractionResponse(
            NpcEntityIds.Lila,
            NpcInteractionKind.Talk,
            memories,
            RelationshipTier.Friend,
            LegacyArchetype.Connector,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("village", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetArchetypeEmotionalBondLine_LinksConnectorToLila()
    {
        var line = NpcEmotionalBondConfig.TryGetArchetypeEmotionalBondLine(
            NpcEntityIds.Lila,
            LegacyArchetype.Connector,
            RelationshipTier.CloseFriend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("connector", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatEmotionalBondHint_ShowsVillageMemoryForLila()
    {
        var memories = new[] { NpcMemoryType.HelpedVillageOften };
        var hint = NpcEmotionalBondConfig.FormatEmotionalBondHint(NpcEntityIds.Lila, memories);

        Assert.NotNull(hint);
        Assert.Contains("village", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void QualifiesForImpact_WorksForLilaWithCompanionMemory()
    {
        var memories = new[] { NpcMemoryType.FrequentLilaCompanion };

        Assert.True(NpcEmotionalBondImpactConfig.QualifiesForImpact(
            NpcEntityIds.Lila,
            RelationshipTier.Friend,
            memories));
    }

    [Fact]
    public void TryGetPersonalAppreciationLine_ReturnsLilaLine()
    {
        var memories = new[] { NpcMemoryType.FrequentLilaCompanion };

        var line = NpcEmotionalBondImpactConfig.TryGetPersonalAppreciationLine(
            NpcEntityIds.Lila,
            memories,
            RelationshipTier.Friend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("Lila", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetFavorItemGrant_ReturnsAppleForVillageHelp()
    {
        var grant = NpcEmotionalBondImpactConfig.TryGetFavorItemGrant(
            NpcEntityIds.Lila,
            NpcMemoryType.HelpedVillageOften,
            RelationshipTier.Friend,
            bondingAction: null,
            variationSeed: 11);

        Assert.NotNull(grant);
        Assert.Equal(ItemType.Apple, grant.Value.ItemType);
    }

    [Fact]
    public void GetAreaHelpMemoryForActivity_MapsVillageToLila()
    {
        Assert.Equal(
            NpcMemoryType.HelpedVillageOften,
            NpcEmotionalBondConfig.GetAreaHelpMemoryForActivity(CommunityActivityKind.HelpVillage));
        Assert.Equal(
            NpcEntityIds.Lila,
            NpcEmotionalBondConfig.GetFocusNpcForActivity(CommunityActivityKind.HelpVillage));
    }

    [Fact]
    public void GetMemoryForAction_MapsLilaBondingActions()
    {
        Assert.Equal(
            NpcMemoryType.CheckedOnLila,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Lila, EmotionalBondActionKind.CheckOn));
        Assert.Equal(
            NpcMemoryType.SpentQuietTimeWithLila,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Lila, EmotionalBondActionKind.SpendTime));
        Assert.Equal(
            NpcMemoryType.ConsciouslyHelpedLila,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Lila, EmotionalBondActionKind.HelpWith));
    }

    [Fact]
    public void ResolveOutcome_LilaReturnsYouthOutcomes()
    {
        var outcome = SocialInfluenceActionConfig.ResolveOutcome(
            playerEntityId: 1,
            NpcEntityIds.Lila,
            variationSeed: 0);

        Assert.True(outcome is SocialInfluenceOutcomeKind.Info
            or SocialInfluenceOutcomeKind.YouthBacking
            or SocialInfluenceOutcomeKind.Item);
    }

    [Fact]
    public void IsSupportedNpc_IncludesLila()
    {
        Assert.True(SocialInfluenceActionConfig.IsSupportedNpc(NpcEntityIds.Lila));
        Assert.Contains(NpcEntityIds.Lila, SocialInfluenceActionConfig.SupportedNpcEntityIds);
    }

    [Fact]
    public void GetInteractionRecoveryBonus_LilaAddsEnergeticExtra()
    {
        var baseTalk = NpcEmotionalBondImpactConfig.GetInteractionRecoveryBonus(
            NpcInteractionKind.Talk,
            RelationshipTier.Friend);
        var lilaTalk = NpcEmotionalBondImpactConfig.GetInteractionRecoveryBonus(
            NpcInteractionKind.Talk,
            RelationshipTier.Friend,
            NpcEntityIds.Lila);

        Assert.True(lilaTalk.MoodBonus > baseTalk.MoodBonus);
        Assert.True(lilaTalk.SocialBonus > baseTalk.SocialBonus);
    }
}