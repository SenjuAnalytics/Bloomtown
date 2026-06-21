using Bloomtown.Shared.Community;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;

namespace Bloomtown.Shared.Tests;

public sealed class NpcGretaEmotionalBondTests
{
    [Fact]
    public void GetCompanionMemoryForNpc_ReturnsFrequentGretaCompanion()
    {
        Assert.Equal(
            NpcMemoryType.FrequentGretaCompanion,
            NpcEmotionalBondConfig.GetCompanionMemoryForNpc(NpcEntityIds.Greta));
    }

    [Fact]
    public void TryGetEmotionalInteractionResponse_ReturnsWarmLineForInnMemory()
    {
        var memories = new[] { NpcMemoryType.HelpedInnOften };

        var line = NpcEmotionalBondConfig.TryGetEmotionalInteractionResponse(
            NpcEntityIds.Greta,
            NpcInteractionKind.Talk,
            memories,
            RelationshipTier.Friend,
            LegacyArchetype.Connector,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("inn", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetArchetypeEmotionalBondLine_LinksConnectorToGreta()
    {
        var line = NpcEmotionalBondConfig.TryGetArchetypeEmotionalBondLine(
            NpcEntityIds.Greta,
            LegacyArchetype.Connector,
            RelationshipTier.CloseFriend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("hearth", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatEmotionalBondHint_ShowsInnMemoryForGreta()
    {
        var memories = new[] { NpcMemoryType.HelpedInnOften };
        var hint = NpcEmotionalBondConfig.FormatEmotionalBondHint(NpcEntityIds.Greta, memories);

        Assert.NotNull(hint);
        Assert.Contains("inn", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void QualifiesForImpact_WorksForGretaWithCompanionMemory()
    {
        var memories = new[] { NpcMemoryType.FrequentGretaCompanion };

        Assert.True(NpcEmotionalBondImpactConfig.QualifiesForImpact(
            NpcEntityIds.Greta,
            RelationshipTier.Friend,
            memories));
    }

    [Fact]
    public void TryGetPersonalAppreciationLine_ReturnsGretaLine()
    {
        var memories = new[] { NpcMemoryType.FrequentGretaCompanion };

        var line = NpcEmotionalBondImpactConfig.TryGetPersonalAppreciationLine(
            NpcEntityIds.Greta,
            memories,
            RelationshipTier.Friend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("Greta", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetFavorItemGrant_ReturnsAppleForInnHelp()
    {
        var grant = NpcEmotionalBondImpactConfig.TryGetFavorItemGrant(
            NpcEntityIds.Greta,
            NpcMemoryType.HelpedInnOften,
            RelationshipTier.Friend,
            bondingAction: null,
            variationSeed: 0);

        Assert.NotNull(grant);
        Assert.Equal(ItemType.Apple, grant.Value.ItemType);
    }

    [Fact]
    public void GetAreaHelpMemoryForActivity_MapsInnToGreta()
    {
        Assert.Equal(
            NpcMemoryType.HelpedInnOften,
            NpcEmotionalBondConfig.GetAreaHelpMemoryForActivity(CommunityActivityKind.HelpInn));
        Assert.Equal(
            NpcEntityIds.Greta,
            NpcEmotionalBondConfig.GetFocusNpcForActivity(CommunityActivityKind.HelpInn));
    }

    [Fact]
    public void GetMemoryForAction_MapsGretaBondingActions()
    {
        Assert.Equal(
            NpcMemoryType.CheckedOnGreta,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Greta, EmotionalBondActionKind.CheckOn));
        Assert.Equal(
            NpcMemoryType.SpentQuietTimeWithGreta,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Greta, EmotionalBondActionKind.SpendTime));
        Assert.Equal(
            NpcMemoryType.ConsciouslyHelpedGreta,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Greta, EmotionalBondActionKind.HelpWith));
    }
}