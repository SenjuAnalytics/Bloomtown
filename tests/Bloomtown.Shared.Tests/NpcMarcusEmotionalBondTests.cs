using Bloomtown.Shared.Community;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Tests;

public sealed class NpcMarcusEmotionalBondTests
{
    [Fact]
    public void GetCompanionMemoryForNpc_ReturnsFrequentMarcusCompanion()
    {
        Assert.Equal(
            NpcMemoryType.FrequentMarcusCompanion,
            NpcEmotionalBondConfig.GetCompanionMemoryForNpc(NpcEntityIds.Marcus));
    }

    [Fact]
    public void TryGetEmotionalInteractionResponse_ReturnsWorkshopLineForWorkshopMemory()
    {
        var memories = new[] { NpcMemoryType.HelpedWorkshopOften };

        var line = NpcEmotionalBondConfig.TryGetEmotionalInteractionResponse(
            NpcEntityIds.Marcus,
            NpcInteractionKind.Talk,
            memories,
            RelationshipTier.Friend,
            LegacyArchetype.Builder,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("workshop", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetArchetypeEmotionalBondLine_LinksBuilderToMarcus()
    {
        var line = NpcEmotionalBondConfig.TryGetArchetypeEmotionalBondLine(
            NpcEntityIds.Marcus,
            LegacyArchetype.Builder,
            RelationshipTier.CloseFriend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("builder", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatEmotionalBondHint_ShowsWorkshopMemoryForMarcus()
    {
        var memories = new[] { NpcMemoryType.HelpedWorkshopOften };
        var hint = NpcEmotionalBondConfig.FormatEmotionalBondHint(NpcEntityIds.Marcus, memories);

        Assert.NotNull(hint);
        Assert.Contains("workshop", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void QualifiesForImpact_WorksForMarcusWithCompanionMemory()
    {
        var memories = new[] { NpcMemoryType.FrequentMarcusCompanion };

        Assert.True(NpcEmotionalBondImpactConfig.QualifiesForImpact(
            NpcEntityIds.Marcus,
            RelationshipTier.Friend,
            memories));
    }

    [Fact]
    public void TryGetPersonalAppreciationLine_ReturnsMarcusLine()
    {
        var memories = new[] { NpcMemoryType.FrequentMarcusCompanion };

        var line = NpcEmotionalBondImpactConfig.TryGetPersonalAppreciationLine(
            NpcEntityIds.Marcus,
            memories,
            RelationshipTier.Friend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("Marcus", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetFavorItemGrant_ReturnsPlankForWorkshopHelp()
    {
        var grant = NpcEmotionalBondImpactConfig.TryGetFavorItemGrant(
            NpcEntityIds.Marcus,
            NpcMemoryType.HelpedWorkshopOften,
            RelationshipTier.Friend,
            bondingAction: null,
            variationSeed: 7);

        Assert.NotNull(grant);
        Assert.Equal(ItemType.Plank, grant.Value.ItemType);
    }

    [Fact]
    public void GetAreaHelpMemoryForActivity_MapsWorkshopToMarcus()
    {
        Assert.Equal(
            NpcMemoryType.HelpedWorkshopOften,
            NpcEmotionalBondConfig.GetAreaHelpMemoryForActivity(CommunityActivityKind.HelpWorkshop));
        Assert.Equal(
            NpcEntityIds.Marcus,
            NpcEmotionalBondConfig.GetFocusNpcForActivity(CommunityActivityKind.HelpWorkshop));
    }

    [Fact]
    public void GetMemoryForAction_MapsMarcusBondingActions()
    {
        Assert.Equal(
            NpcMemoryType.CheckedOnMarcus,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Marcus, EmotionalBondActionKind.CheckOn));
        Assert.Equal(
            NpcMemoryType.SpentQuietTimeWithMarcus,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Marcus, EmotionalBondActionKind.SpendTime));
        Assert.Equal(
            NpcMemoryType.ConsciouslyHelpedMarcus,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Marcus, EmotionalBondActionKind.HelpWith));
    }

    [Fact]
    public void ResolveOutcome_MarcusReturnsCraftingOutcomes()
    {
        var outcome = SocialInfluenceActionConfig.ResolveOutcome(
            playerEntityId: 1,
            NpcEntityIds.Marcus,
            variationSeed: 0);

        Assert.True(outcome is SocialInfluenceOutcomeKind.Info
            or SocialInfluenceOutcomeKind.CraftingBacking
            or SocialInfluenceOutcomeKind.Item);
    }

    [Fact]
    public void IsSupportedNpc_IncludesMarcus()
    {
        Assert.True(SocialInfluenceActionConfig.IsSupportedNpc(NpcEntityIds.Marcus));
        Assert.Contains(NpcEntityIds.Marcus, SocialInfluenceActionConfig.SupportedNpcEntityIds);
    }

    [Fact]
    public void GetInteractionBonus_MarcusAddsWarmCraftsmanExtra()
    {
        var baseTalk = VillageSocialStandingImpactConfig.GetInteractionBonus(
            VillageSocialStandingTier.WellLiked,
            NpcEntityIds.Mira);
        var marcusTalk = VillageSocialStandingImpactConfig.GetInteractionBonus(
            VillageSocialStandingTier.WellLiked,
            NpcEntityIds.Marcus);

        Assert.True(marcusTalk.MoodBonus > baseTalk.MoodBonus);
        Assert.True(marcusTalk.SocialBonus > baseTalk.SocialBonus);
    }
}