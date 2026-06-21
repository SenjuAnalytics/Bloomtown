using Bloomtown.Shared.Community;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Tests;

public sealed class NpcBenEmotionalBondTests
{
    [Fact]
    public void GetCompanionMemoryForNpc_ReturnsFrequentBenCompanion()
    {
        Assert.Equal(
            NpcMemoryType.FrequentBenCompanion,
            NpcEmotionalBondConfig.GetCompanionMemoryForNpc(NpcEntityIds.Ben));
    }

    [Fact]
    public void TryGetEmotionalInteractionResponse_ReturnsPatrolLineForPatrolMemory()
    {
        var memories = new[] { NpcMemoryType.HelpedPatrolOften };

        var line = NpcEmotionalBondConfig.TryGetEmotionalInteractionResponse(
            NpcEntityIds.Ben,
            NpcInteractionKind.Talk,
            memories,
            RelationshipTier.Friend,
            LegacyArchetype.Caretaker,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("patrol", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetArchetypeEmotionalBondLine_LinksCaretakerToBen()
    {
        var line = NpcEmotionalBondConfig.TryGetArchetypeEmotionalBondLine(
            NpcEntityIds.Ben,
            LegacyArchetype.Caretaker,
            RelationshipTier.CloseFriend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("caretaker", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatEmotionalBondHint_ShowsPatrolMemoryForBen()
    {
        var memories = new[] { NpcMemoryType.HelpedPatrolOften };
        var hint = NpcEmotionalBondConfig.FormatEmotionalBondHint(NpcEntityIds.Ben, memories);

        Assert.NotNull(hint);
        Assert.Contains("patrol", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void QualifiesForImpact_WorksForBenWithCompanionMemory()
    {
        var memories = new[] { NpcMemoryType.FrequentBenCompanion };

        Assert.True(NpcEmotionalBondImpactConfig.QualifiesForImpact(
            NpcEntityIds.Ben,
            RelationshipTier.Friend,
            memories));
    }

    [Fact]
    public void TryGetPersonalAppreciationLine_ReturnsBenLine()
    {
        var memories = new[] { NpcMemoryType.FrequentBenCompanion };

        var line = NpcEmotionalBondImpactConfig.TryGetPersonalAppreciationLine(
            NpcEntityIds.Ben,
            memories,
            RelationshipTier.Friend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("Ben", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetFavorItemGrant_ReturnsWoodForPatrolHelp()
    {
        var grant = NpcEmotionalBondImpactConfig.TryGetFavorItemGrant(
            NpcEntityIds.Ben,
            NpcMemoryType.HelpedPatrolOften,
            RelationshipTier.Friend,
            bondingAction: null,
            variationSeed: 0);

        Assert.NotNull(grant);
        Assert.Equal(ItemType.Wood, grant.Value.ItemType);
    }

    [Fact]
    public void GetAreaHelpMemoryForActivity_MapsPatrolToBen()
    {
        Assert.Equal(
            NpcMemoryType.HelpedPatrolOften,
            NpcEmotionalBondConfig.GetAreaHelpMemoryForActivity(CommunityActivityKind.HelpPatrol));
        Assert.Equal(
            NpcEntityIds.Ben,
            NpcEmotionalBondConfig.GetFocusNpcForActivity(CommunityActivityKind.HelpPatrol));
    }

    [Fact]
    public void GetMemoryForAction_MapsBenBondingActions()
    {
        Assert.Equal(
            NpcMemoryType.CheckedOnBen,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Ben, EmotionalBondActionKind.CheckOn));
        Assert.Equal(
            NpcMemoryType.SpentQuietTimeWithBen,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Ben, EmotionalBondActionKind.SpendTime));
        Assert.Equal(
            NpcMemoryType.ConsciouslyHelpedBen,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Ben, EmotionalBondActionKind.HelpWith));
    }

    [Fact]
    public void ResolveOutcome_BenReturnsGuardOutcomes()
    {
        var outcome = SocialInfluenceActionConfig.ResolveOutcome(
            playerEntityId: 1,
            NpcEntityIds.Ben,
            variationSeed: 0);

        Assert.True(outcome is SocialInfluenceOutcomeKind.Info
            or SocialInfluenceOutcomeKind.GuardBacking
            or SocialInfluenceOutcomeKind.Item);
    }

    [Fact]
    public void IsSupportedNpc_IncludesBen()
    {
        Assert.True(SocialInfluenceActionConfig.IsSupportedNpc(NpcEntityIds.Ben));
        Assert.Contains(NpcEntityIds.Ben, SocialInfluenceActionConfig.SupportedNpcEntityIds);
    }
}