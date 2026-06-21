using Bloomtown.Shared.Community;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;

namespace Bloomtown.Shared.Tests;

public sealed class NpcEliasEmotionalBondTests
{
    [Fact]
    public void GetCompanionMemoryForNpc_ReturnsFrequentEliasCompanion()
    {
        Assert.Equal(
            NpcMemoryType.FrequentEliasCompanion,
            NpcEmotionalBondConfig.GetCompanionMemoryForNpc(NpcEntityIds.Elias));
    }

    [Fact]
    public void TryGetEmotionalInteractionResponse_ReturnsForgeLineForSmithyMemory()
    {
        var memories = new[] { NpcMemoryType.HelpedSmithyOften };

        var line = NpcEmotionalBondConfig.TryGetEmotionalInteractionResponse(
            NpcEntityIds.Elias,
            NpcInteractionKind.Talk,
            memories,
            RelationshipTier.Friend,
            LegacyArchetype.Builder,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("smithy", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetArchetypeEmotionalBondLine_LinksBuilderToElias()
    {
        var line = NpcEmotionalBondConfig.TryGetArchetypeEmotionalBondLine(
            NpcEntityIds.Elias,
            LegacyArchetype.Builder,
            RelationshipTier.CloseFriend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("builder", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatEmotionalBondHint_ShowsSmithyMemoryForElias()
    {
        var memories = new[] { NpcMemoryType.HelpedSmithyOften };
        var hint = NpcEmotionalBondConfig.FormatEmotionalBondHint(NpcEntityIds.Elias, memories);

        Assert.NotNull(hint);
        Assert.Contains("smithy", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void QualifiesForImpact_WorksForEliasWithCompanionMemory()
    {
        var memories = new[] { NpcMemoryType.FrequentEliasCompanion };

        Assert.True(NpcEmotionalBondImpactConfig.QualifiesForImpact(
            NpcEntityIds.Elias,
            RelationshipTier.Friend,
            memories));
    }

    [Fact]
    public void TryGetPersonalAppreciationLine_ReturnsEliasLine()
    {
        var memories = new[] { NpcMemoryType.FrequentEliasCompanion };

        var line = NpcEmotionalBondImpactConfig.TryGetPersonalAppreciationLine(
            NpcEntityIds.Elias,
            memories,
            RelationshipTier.Friend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("Elias", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetFavorItemGrant_ReturnsWoodForSmithyHelp()
    {
        var grant = NpcEmotionalBondImpactConfig.TryGetFavorItemGrant(
            NpcEntityIds.Elias,
            NpcMemoryType.HelpedSmithyOften,
            RelationshipTier.Friend,
            bondingAction: null,
            variationSeed: 51);

        Assert.NotNull(grant);
        Assert.Equal(ItemType.Wood, grant.Value.ItemType);
    }

    [Fact]
    public void GetAreaHelpMemoryForActivity_MapsSmithyToElias()
    {
        Assert.Equal(
            NpcMemoryType.HelpedSmithyOften,
            NpcEmotionalBondConfig.GetAreaHelpMemoryForActivity(CommunityActivityKind.HelpSmithy));
        Assert.Equal(
            NpcEntityIds.Elias,
            NpcEmotionalBondConfig.GetFocusNpcForActivity(CommunityActivityKind.HelpSmithy));
    }

    [Fact]
    public void GetMemoryForAction_MapsEliasBondingActions()
    {
        Assert.Equal(
            NpcMemoryType.CheckedOnElias,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Elias, EmotionalBondActionKind.CheckOn));
        Assert.Equal(
            NpcMemoryType.SpentQuietTimeWithElias,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Elias, EmotionalBondActionKind.SpendTime));
        Assert.Equal(
            NpcMemoryType.ConsciouslyHelpedElias,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Elias, EmotionalBondActionKind.HelpWith));
    }
}