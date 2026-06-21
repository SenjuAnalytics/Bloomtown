using Bloomtown.Shared.Community;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;

namespace Bloomtown.Shared.Tests;

public sealed class NpcEmotionalBondTests
{
    [Fact]
    public void IsFocusNpc_IncludesElsieHaroldMiraTomGretaNoraEliasBenLilaRowanAndMarcus()
    {
        Assert.True(NpcEmotionalBondConfig.IsFocusNpc(NpcEntityIds.Elsie));
        Assert.True(NpcEmotionalBondConfig.IsFocusNpc(NpcEntityIds.Harold));
        Assert.True(NpcEmotionalBondConfig.IsFocusNpc(NpcEntityIds.Mira));
        Assert.True(NpcEmotionalBondConfig.IsFocusNpc(NpcEntityIds.Tom));
        Assert.True(NpcEmotionalBondConfig.IsFocusNpc(NpcEntityIds.Greta));
        Assert.True(NpcEmotionalBondConfig.IsFocusNpc(NpcEntityIds.Nora));
        Assert.True(NpcEmotionalBondConfig.IsFocusNpc(NpcEntityIds.Elias));
        Assert.True(NpcEmotionalBondConfig.IsFocusNpc(NpcEntityIds.Ben));
        Assert.True(NpcEmotionalBondConfig.IsFocusNpc(NpcEntityIds.Lila));
        Assert.True(NpcEmotionalBondConfig.IsFocusNpc(NpcEntityIds.Rowan));
        Assert.True(NpcEmotionalBondConfig.IsFocusNpc(NpcEntityIds.Marcus));
        Assert.True(NpcEmotionalBondConfig.IsFocusNpc(NpcEntityIds.Eleanor));
        Assert.Equal(12, NpcEmotionalBondConfig.FocusNpcEntityIds.Length);
        Assert.False(NpcEmotionalBondConfig.IsFocusNpc(10_099));
    }

    [Fact]
    public void TryGetEmotionalInteractionResponse_RequiresFriendTierAndFocusMemory()
    {
        var memories = new[] { NpcMemoryType.FrequentElsieCompanion };

        Assert.Null(NpcEmotionalBondConfig.TryGetEmotionalInteractionResponse(
            NpcEntityIds.Elsie,
            NpcInteractionKind.Talk,
            memories,
            RelationshipTier.Acquaintance,
            LegacyArchetype.None,
            0));

        var line = NpcEmotionalBondConfig.TryGetEmotionalInteractionResponse(
            NpcEntityIds.Elsie,
            NpcInteractionKind.Talk,
            memories,
            RelationshipTier.Friend,
            LegacyArchetype.Caretaker,
            0);

        Assert.NotNull(line);
        Assert.Contains("Bloomtown", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetEmotionalInteractionResponse_DiffersBetweenElsieAndHarold()
    {
        var elsieMemories = new[] { NpcMemoryType.HelpedGardenOften };
        var haroldMemories = new[] { NpcMemoryType.HelpedWellOften };

        var elsie = NpcEmotionalBondConfig.TryGetEmotionalInteractionResponse(
            NpcEntityIds.Elsie,
            NpcInteractionKind.Talk,
            elsieMemories,
            RelationshipTier.Friend,
            LegacyArchetype.Caretaker,
            0);
        var harold = NpcEmotionalBondConfig.TryGetEmotionalInteractionResponse(
            NpcEntityIds.Harold,
            NpcInteractionKind.Talk,
            haroldMemories,
            RelationshipTier.Friend,
            LegacyArchetype.Caretaker,
            0);

        Assert.NotNull(elsie);
        Assert.NotNull(harold);
        Assert.NotEqual(elsie, harold);
    }

    [Fact]
    public void TryGetArchetypeEmotionalBondLine_LinksLegacyToPersonalBond()
    {
        var line = NpcEmotionalBondConfig.TryGetArchetypeEmotionalBondLine(
            NpcEntityIds.Elsie,
            LegacyArchetype.Caretaker,
            RelationshipTier.Friend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("caretaker", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetEmotionalPersonalMoment_ReturnsWarmLineForElsie()
    {
        var memories = new[] { NpcMemoryType.HelpedGardenOften };

        var line = NpcEmotionalBondConfig.TryGetEmotionalPersonalMoment(
            NpcEntityIds.Elsie,
            memories,
            RelationshipTier.Friend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.False(string.IsNullOrWhiteSpace(line));
    }

    [Fact]
    public void GetAreaHelpMemoryForActivity_MapsGardenAndWell()
    {
        Assert.Equal(
            NpcMemoryType.HelpedGardenOften,
            NpcEmotionalBondConfig.GetAreaHelpMemoryForActivity(CommunityActivityKind.HelpGarden));
        Assert.Equal(
            NpcMemoryType.HelpedWellOften,
            NpcEmotionalBondConfig.GetAreaHelpMemoryForActivity(CommunityActivityKind.HelpWell));
        Assert.Equal(
            NpcMemoryType.HelpedMarketOften,
            NpcEmotionalBondConfig.GetAreaHelpMemoryForActivity(CommunityActivityKind.HelpMarket));
        Assert.Equal(
            NpcEntityIds.Mira,
            NpcEmotionalBondConfig.GetFocusNpcForActivity(CommunityActivityKind.HelpMarket));
        Assert.Equal(
            NpcMemoryType.HelpedLumberOften,
            NpcEmotionalBondConfig.GetAreaHelpMemoryForActivity(CommunityActivityKind.HelpLumber));
        Assert.Equal(
            NpcEntityIds.Tom,
            NpcEmotionalBondConfig.GetFocusNpcForActivity(CommunityActivityKind.HelpLumber));
        Assert.Equal(
            NpcMemoryType.HelpedInnOften,
            NpcEmotionalBondConfig.GetAreaHelpMemoryForActivity(CommunityActivityKind.HelpInn));
        Assert.Equal(
            NpcEntityIds.Greta,
            NpcEmotionalBondConfig.GetFocusNpcForActivity(CommunityActivityKind.HelpInn));
    }

    [Fact]
    public void FormatEmotionalBondHint_ShowsGardenMemoryForElsie()
    {
        var memories = new[] { NpcMemoryType.HelpedGardenOften };
        var hint = NpcEmotionalBondConfig.FormatEmotionalBondHint(NpcEntityIds.Elsie, memories);

        Assert.NotNull(hint);
        Assert.Contains("garden", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShouldTriggerEmotionalPersonalMoment_UsesLowThreshold()
    {
        var roll = (5u * 43 + 2u * 29 + (uint)(100 % 983)) % 100;
        Assert.Equal(
            roll < NpcEmotionalBondConfig.EmotionalMomentChancePercent,
            NpcEmotionalBondConfig.ShouldTriggerEmotionalPersonalMoment(5, 2, 100));
    }
}