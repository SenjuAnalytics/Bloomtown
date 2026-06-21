using Bloomtown.Shared.Community;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;

namespace Bloomtown.Shared.Tests;

public sealed class NpcNoraEmotionalBondTests
{
    [Fact]
    public void GetCompanionMemoryForNpc_ReturnsFrequentNoraCompanion()
    {
        Assert.Equal(
            NpcMemoryType.FrequentNoraCompanion,
            NpcEmotionalBondConfig.GetCompanionMemoryForNpc(NpcEntityIds.Nora));
    }

    [Fact]
    public void TryGetEmotionalInteractionResponse_ReturnsCalmLineForHerbMemory()
    {
        var memories = new[] { NpcMemoryType.HelpedHerbGardenOften };

        var line = NpcEmotionalBondConfig.TryGetEmotionalInteractionResponse(
            NpcEntityIds.Nora,
            NpcInteractionKind.Talk,
            memories,
            RelationshipTier.Friend,
            LegacyArchetype.Caretaker,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("herb", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetArchetypeEmotionalBondLine_LinksCaretakerToNora()
    {
        var line = NpcEmotionalBondConfig.TryGetArchetypeEmotionalBondLine(
            NpcEntityIds.Nora,
            LegacyArchetype.Caretaker,
            RelationshipTier.CloseFriend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("herb", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatEmotionalBondHint_ShowsHerbMemoryForNora()
    {
        var memories = new[] { NpcMemoryType.HelpedHerbGardenOften };
        var hint = NpcEmotionalBondConfig.FormatEmotionalBondHint(NpcEntityIds.Nora, memories);

        Assert.NotNull(hint);
        Assert.Contains("herb", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void QualifiesForImpact_WorksForNoraWithCompanionMemory()
    {
        var memories = new[] { NpcMemoryType.FrequentNoraCompanion };

        Assert.True(NpcEmotionalBondImpactConfig.QualifiesForImpact(
            NpcEntityIds.Nora,
            RelationshipTier.Friend,
            memories));
    }

    [Fact]
    public void TryGetPersonalAppreciationLine_ReturnsNoraLine()
    {
        var memories = new[] { NpcMemoryType.FrequentNoraCompanion };

        var line = NpcEmotionalBondImpactConfig.TryGetPersonalAppreciationLine(
            NpcEntityIds.Nora,
            memories,
            RelationshipTier.Friend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("Nora", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetFavorItemGrant_ReturnsAppleForHerbHelp()
    {
        var grant = NpcEmotionalBondImpactConfig.TryGetFavorItemGrant(
            NpcEntityIds.Nora,
            NpcMemoryType.HelpedHerbGardenOften,
            RelationshipTier.Friend,
            bondingAction: null,
            variationSeed: 3);

        Assert.NotNull(grant);
        Assert.Equal(ItemType.Apple, grant.Value.ItemType);
    }

    [Fact]
    public void GetAreaHelpMemoryForActivity_MapsHerbGardenToNora()
    {
        Assert.Equal(
            NpcMemoryType.HelpedHerbGardenOften,
            NpcEmotionalBondConfig.GetAreaHelpMemoryForActivity(CommunityActivityKind.HelpHerbGarden));
        Assert.Equal(
            NpcEntityIds.Nora,
            NpcEmotionalBondConfig.GetFocusNpcForActivity(CommunityActivityKind.HelpHerbGarden));
    }

    [Fact]
    public void GetMemoryForAction_MapsNoraBondingActions()
    {
        Assert.Equal(
            NpcMemoryType.CheckedOnNora,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Nora, EmotionalBondActionKind.CheckOn));
        Assert.Equal(
            NpcMemoryType.SpentQuietTimeWithNora,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Nora, EmotionalBondActionKind.SpendTime));
        Assert.Equal(
            NpcMemoryType.ConsciouslyHelpedNora,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Nora, EmotionalBondActionKind.HelpWith));
    }
}