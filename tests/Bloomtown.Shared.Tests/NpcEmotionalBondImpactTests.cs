using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Items;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Tests;

public sealed class NpcEmotionalBondImpactTests
{
    [Fact]
    public void QualifiesForImpact_RequiresFocusNpcFriendTierAndMemory()
    {
        var memories = new[] { NpcMemoryType.FrequentElsieCompanion };

        Assert.False(NpcEmotionalBondImpactConfig.QualifiesForImpact(
            10_099,
            RelationshipTier.Friend,
            memories));
        Assert.False(NpcEmotionalBondImpactConfig.QualifiesForImpact(
            NpcEntityIds.Elsie,
            RelationshipTier.Acquaintance,
            memories));
        Assert.False(NpcEmotionalBondImpactConfig.QualifiesForImpact(
            NpcEntityIds.Elsie,
            RelationshipTier.Friend,
            Array.Empty<NpcMemoryType>()));
        Assert.True(NpcEmotionalBondImpactConfig.QualifiesForImpact(
            NpcEntityIds.Elsie,
            RelationshipTier.Friend,
            memories));
    }

    [Fact]
    public void GetInteractionRecoveryBonus_TalkGreaterThanGreet()
    {
        var greet = NpcEmotionalBondImpactConfig.GetInteractionRecoveryBonus(NpcInteractionKind.Greet);
        var talk = NpcEmotionalBondImpactConfig.GetInteractionRecoveryBonus(NpcInteractionKind.Talk);

        Assert.True(talk.MoodBonus > greet.MoodBonus);
        Assert.True(talk.SocialBonus > greet.SocialBonus);
    }

    [Fact]
    public void GetInteractionRecoveryBonus_FriendTierInMeaningfulRange()
    {
        var talk = NpcEmotionalBondImpactConfig.GetInteractionRecoveryBonus(
            NpcInteractionKind.Talk,
            RelationshipTier.Friend);

        Assert.InRange(talk.MoodBonus, 6f, 8f);
        Assert.InRange(talk.SocialBonus, 8f, 10f);
    }

    [Fact]
    public void GetBondingActionRecoveryBonus_ShareMomentIsStrongest()
    {
        var checkOn = NpcEmotionalBondImpactConfig.GetBondingActionRecoveryBonus(EmotionalBondActionKind.CheckOn);
        var share = NpcEmotionalBondImpactConfig.GetBondingActionRecoveryBonus(EmotionalBondActionKind.ShareMoment);

        Assert.True(share.MoodBonus > checkOn.MoodBonus);
        Assert.True(share.SocialBonus > checkOn.SocialBonus);
    }

    [Fact]
    public void GetBondingActionRecoveryBonus_CloseFriendShareMomentInStrongRange()
    {
        var share = NpcEmotionalBondImpactConfig.GetBondingActionRecoveryBonus(
            EmotionalBondActionKind.ShareMoment,
            RelationshipTier.CloseFriend);

        Assert.InRange(share.MoodBonus, 9f, 13f);
        Assert.InRange(share.SocialBonus, 12f, 17f);
    }

    [Fact]
    public void TryGetPersonalAppreciationLine_ReturnsWarmLineForCloseElsie()
    {
        var memories = new[] { NpcMemoryType.FrequentElsieCompanion };

        var line = NpcEmotionalBondImpactConfig.TryGetPersonalAppreciationLine(
            NpcEntityIds.Elsie,
            memories,
            RelationshipTier.CloseFriend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("Elsie", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetPersonalAppreciationLine_ReferencesCheckedOnMemory()
    {
        var memories = new[] { NpcMemoryType.CheckedOnHarold };

        var line = NpcEmotionalBondImpactConfig.TryGetPersonalAppreciationLine(
            NpcEntityIds.Harold,
            memories,
            RelationshipTier.Friend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("checked on", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetEmotionalBondInfoTip_ReturnsLightVillageHint()
    {
        var tip = NpcEmotionalBondImpactConfig.TryGetEmotionalBondInfoTip(
            NpcEntityIds.Harold,
            GameTimeOfDay.Evening,
            LegacyArchetype.None,
            variationSeed: 0);

        Assert.NotNull(tip);
        Assert.Contains("Harold", tip, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetEmotionalBondInfoTip_ConnectorGetsSocialRichTip()
    {
        var tip = NpcEmotionalBondImpactConfig.TryGetEmotionalBondInfoTip(
            NpcEntityIds.Mira,
            GameTimeOfDay.Afternoon,
            LegacyArchetype.Connector,
            variationSeed: 0);

        Assert.NotNull(tip);
        Assert.Contains("Mira", tip, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetHelpfulFavorLine_ReturnsPracticalLineForHarold()
    {
        var memories = new[] { NpcMemoryType.HelpedWellOften };

        var line = NpcEmotionalBondImpactConfig.TryGetHelpfulFavorLine(
            NpcEntityIds.Harold,
            NpcInteractionKind.Talk,
            memories,
            RelationshipTier.Friend,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("Harold", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatNeedsRecoveryFeedback_LinksRecoveryToNpcBond()
    {
        var feedback = NpcEmotionalBondImpactConfig.FormatNeedsRecoveryFeedback("Elsie", 7f, 9f);

        Assert.Contains("Elsie", feedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("bond", feedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("warmer", feedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("mood", feedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("social", feedback, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetInteractionRecoveryBonus_CloseFriendAddsExtraRecovery()
    {
        var friend = NpcEmotionalBondImpactConfig.GetInteractionRecoveryBonus(
            NpcInteractionKind.Talk,
            RelationshipTier.Friend);
        var close = NpcEmotionalBondImpactConfig.GetInteractionRecoveryBonus(
            NpcInteractionKind.Talk,
            RelationshipTier.CloseFriend);

        Assert.True(close.MoodBonus > friend.MoodBonus);
        Assert.True(close.SocialBonus > friend.SocialBonus);
    }

    [Fact]
    public void FormatBondInfoFeedback_ExplainsTrust()
    {
        var feedback = NpcEmotionalBondImpactConfig.FormatBondInfoFeedback(
            "Harold",
            "Harold mentions the well.");

        Assert.Contains("Harold", feedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("close", feedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("personal", feedback, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatBondAppreciationFeedback_ExplainsEmotionalBond()
    {
        var feedback = NpcEmotionalBondImpactConfig.FormatBondAppreciationFeedback(
            "Mira",
            "Mira smiles warmly.");

        Assert.Contains("Mira", feedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("stranger", feedback, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShouldTriggerEmotionalBondInfo_UsesLowThreshold()
    {
        var roll = (5u * 71 + 4u * 53 + (uint)(300 % 881)) % 100;
        Assert.Equal(
            roll < NpcEmotionalBondImpactConfig.EmotionalBondInfoChancePercent,
            NpcEmotionalBondImpactConfig.ShouldTriggerEmotionalBondInfo(5, 4, 300));
    }

    [Fact]
    public void ShouldTriggerEmotionalBondInfo_ConnectorHasHigherThreshold()
    {
        var roll = (5u * 71 + 4u * 53 + (uint)(300 % 881)) % 100;
        var baseThreshold = NpcEmotionalBondImpactConfig.EmotionalBondInfoChancePercent;
        var connectorThreshold = baseThreshold + NpcEmotionalBondImpactConfig.ConnectorInfoChanceBonusPercent;

        Assert.Equal(roll < baseThreshold,
            NpcEmotionalBondImpactConfig.ShouldTriggerEmotionalBondInfo(5, 4, 300, LegacyArchetype.None));
        Assert.Equal(roll < connectorThreshold,
            NpcEmotionalBondImpactConfig.ShouldTriggerEmotionalBondInfo(5, 4, 300, LegacyArchetype.Connector));
    }

    [Fact]
    public void TryGetFavorItemGrant_ReturnsSmallItemForCloseFriendGardenHelp()
    {
        var grant = NpcEmotionalBondImpactConfig.TryGetFavorItemGrant(
            NpcEntityIds.Elsie,
            NpcMemoryType.HelpedGardenOften,
            RelationshipTier.CloseFriend,
            bondingAction: null,
            variationSeed: 2);

        Assert.NotNull(grant);
        Assert.Equal(ItemType.Apple, grant.Value.ItemType);
        Assert.Equal(2, grant.Value.Quantity);
    }

    [Fact]
    public void FormatBondItemGrantFeedback_MentionsBond()
    {
        var feedback = NpcEmotionalBondImpactConfig.FormatBondItemGrantFeedback(
            "Harold",
            new EmotionalBondFavorGrant(ItemType.Wood, 1));

        Assert.Contains("Harold", feedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("bond", feedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Wood", feedback, StringComparison.OrdinalIgnoreCase);
    }
}