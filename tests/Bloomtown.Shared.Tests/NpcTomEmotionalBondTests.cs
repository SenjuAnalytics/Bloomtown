using Bloomtown.Shared.Community;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Tests;

public sealed class NpcTomEmotionalBondTests
{
    [Fact]
    public void GetCompanionMemoryForNpc_ReturnsFrequentTomCompanion()
    {
        Assert.Equal(
            NpcMemoryType.FrequentTomCompanion,
            NpcEmotionalBondConfig.GetCompanionMemoryForNpc(NpcEntityIds.Tom));
    }

    [Fact]
    public void TryGetEmotionalInteractionResponse_ReturnsPracticalLineForLumberMemory()
    {
        var memories = new[] { NpcMemoryType.HelpedLumberOften };

        var line = NpcEmotionalBondConfig.TryGetEmotionalInteractionResponse(
            NpcEntityIds.Tom,
            NpcInteractionKind.Talk,
            memories,
            RelationshipTier.Friend,
            LegacyArchetype.Builder,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("yard", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetArchetypeEmotionalBondLine_LinksBuilderToTom()
    {
        var line = NpcEmotionalBondConfig.TryGetArchetypeEmotionalBondLine(
            NpcEntityIds.Tom,
            LegacyArchetype.Builder,
            RelationshipTier.Friend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("builder", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatEmotionalBondHint_ShowsLumberMemoryForTom()
    {
        var memories = new[] { NpcMemoryType.HelpedLumberOften };
        var hint = NpcEmotionalBondConfig.FormatEmotionalBondHint(NpcEntityIds.Tom, memories);

        Assert.NotNull(hint);
        Assert.Contains("lumber", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void QualifiesForImpact_WorksForTomWithCompanionMemory()
    {
        var memories = new[] { NpcMemoryType.FrequentTomCompanion };

        Assert.True(NpcEmotionalBondImpactConfig.QualifiesForImpact(
            NpcEntityIds.Tom,
            RelationshipTier.Friend,
            memories));
    }

    [Fact]
    public void TryGetPersonalAppreciationLine_ReturnsTomLine()
    {
        var memories = new[] { NpcMemoryType.FrequentTomCompanion };

        var line = NpcEmotionalBondImpactConfig.TryGetPersonalAppreciationLine(
            NpcEntityIds.Tom,
            memories,
            RelationshipTier.Friend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("Tom", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetFavorItemGrant_ReturnsWoodForLumberHelp()
    {
        var grant = NpcEmotionalBondImpactConfig.TryGetFavorItemGrant(
            NpcEntityIds.Tom,
            NpcMemoryType.HelpedLumberOften,
            RelationshipTier.Friend,
            bondingAction: null,
            variationSeed: 15);

        Assert.NotNull(grant);
        Assert.Equal(ItemType.Wood, grant.Value.ItemType);
    }

    [Fact]
    public void GetAreaHelpMemoryForActivity_MapsLumberToTom()
    {
        Assert.Equal(
            NpcMemoryType.HelpedLumberOften,
            NpcEmotionalBondConfig.GetAreaHelpMemoryForActivity(CommunityActivityKind.HelpLumber));
        Assert.Equal(
            NpcEntityIds.Tom,
            NpcEmotionalBondConfig.GetFocusNpcForActivity(CommunityActivityKind.HelpLumber));
    }
}