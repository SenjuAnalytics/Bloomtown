using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;

namespace Bloomtown.Shared.Tests;

public sealed class NpcEmotionalBondAgencyTests
{
    [Fact]
    public void GetMemoryForAction_MapsCheckOnShareMomentAndHelp()
    {
        Assert.Equal(
            NpcMemoryType.CheckedOnElsie,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Elsie, EmotionalBondActionKind.CheckOn));
        Assert.Equal(
            NpcMemoryType.SharedMomentWithHarold,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Harold, EmotionalBondActionKind.ShareMoment));
        Assert.Equal(
            NpcMemoryType.ConsciouslyHelpedElsie,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Elsie, EmotionalBondActionKind.HelpWith));
        Assert.Equal(
            NpcMemoryType.CheckedOnMira,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Mira, EmotionalBondActionKind.CheckOn));
        Assert.Equal(
            NpcMemoryType.CheckedOnTom,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Tom, EmotionalBondActionKind.CheckOn));
        Assert.Equal(
            NpcMemoryType.SpentQuietTimeWithElsie,
            NpcEmotionalBondAgencyConfig.GetMemoryForAction(NpcEntityIds.Elsie, EmotionalBondActionKind.SpendTime));
    }

    [Fact]
    public void IsValidAction_IncludesSpendTime()
    {
        Assert.True(NpcEmotionalBondAgencyConfig.IsValidAction(EmotionalBondActionKind.SpendTime));
    }

    [Fact]
    public void GetBondingActionRecoveryBonus_SpendTimeGreaterThanCheckOn()
    {
        var checkOn = NpcEmotionalBondImpactConfig.GetBondingActionRecoveryBonus(EmotionalBondActionKind.CheckOn);
        var spendTime = NpcEmotionalBondImpactConfig.GetBondingActionRecoveryBonus(EmotionalBondActionKind.SpendTime);

        Assert.True(spendTime.MoodBonus > checkOn.MoodBonus);
        Assert.True(spendTime.SocialBonus > checkOn.SocialBonus);
    }

    [Fact]
    public void GetMinTier_ShareMomentRequiresFriend()
    {
        Assert.Equal(RelationshipTier.Acquaintance, NpcEmotionalBondAgencyConfig.GetMinTier(EmotionalBondActionKind.CheckOn));
        Assert.Equal(RelationshipTier.Friend, NpcEmotionalBondAgencyConfig.GetMinTier(EmotionalBondActionKind.ShareMoment));
    }

    [Fact]
    public void TryGetBondingActionResponse_ReturnsWarmLineForElsieCheckOn()
    {
        var line = NpcEmotionalBondAgencyConfig.TryGetBondingActionResponse(
            NpcEntityIds.Elsie,
            EmotionalBondActionKind.CheckOn,
            RelationshipTier.Friend,
            LegacyArchetype.Caretaker,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("Elsie", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetNpcRemembranceLine_RecallsGardenHelpForElsie()
    {
        var memories = new[] { NpcMemoryType.HelpedGardenOften };

        var line = NpcEmotionalBondAgencyConfig.TryGetNpcRemembranceLine(
            NpcEntityIds.Elsie,
            memories,
            RelationshipTier.Friend,
            LegacyArchetype.None,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("garden", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void BuildBondDeepenedFeedback_NotesNewMemory()
    {
        var feedback = NpcEmotionalBondAgencyConfig.BuildBondDeepenedFeedback(
            "Elsie",
            EmotionalBondActionKind.CheckOn,
            recordedNewMemory: true,
            tierIncreased: false,
            RelationshipTier.Friend);

        Assert.Contains("Elsie", feedback, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("remember", feedback, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetArchetypeBondingHint_LinksCaretakerToCheckOn()
    {
        var hint = NpcEmotionalBondAgencyConfig.TryGetArchetypeBondingHint(
            LegacyArchetype.Caretaker,
            EmotionalBondActionKind.CheckOn,
            variationSeed: 0);

        Assert.NotNull(hint);
        Assert.Contains("caretaker", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatEmotionalBondHint_ShowsCheckedOnMemory()
    {
        var memories = new[] { NpcMemoryType.CheckedOnHarold };
        var hint = NpcEmotionalBondConfig.FormatEmotionalBondHint(NpcEntityIds.Harold, memories);

        Assert.NotNull(hint);
        Assert.Contains("check", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShouldTriggerNpcRemembrance_UsesConfiguredThreshold()
    {
        var roll = (5u * 67 + 4u * 47 + (uint)(200 % 907)) % 100;
        Assert.Equal(
            roll < NpcEmotionalBondAgencyConfig.NpcRemembranceChancePercent,
            NpcEmotionalBondAgencyConfig.ShouldTriggerNpcRemembrance(5, 4, 200));
    }
}