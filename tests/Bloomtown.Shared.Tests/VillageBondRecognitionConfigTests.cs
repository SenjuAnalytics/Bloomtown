using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Tests;

public sealed class VillageBondRecognitionConfigTests
{
    [Fact]
    public void CountFocusCloseFriends_OnlyCountsFocusNpcs()
    {
        var tiers = new Dictionary<uint, RelationshipTier>
        {
            [NpcEntityIds.Elsie] = RelationshipTier.CloseFriend,
            [NpcEntityIds.Harold] = RelationshipTier.Friend,
            [NpcEntityIds.Mira] = RelationshipTier.CloseFriend,
            [NpcEntityIds.Tom] = RelationshipTier.Acquaintance,
            [10_099] = RelationshipTier.CloseFriend,
        };

        Assert.Equal(2, VillageBondRecognitionConfig.CountFocusCloseFriends(id => tiers.GetValueOrDefault(id)));
    }

    [Fact]
    public void ShouldTriggerAmbientRecognition_ScalesWithCloseFriendCount()
    {
        var oneFriendRoll = (5u * 97 + (uint)(100 % 919) + 3u * 11) % 100;
        var threeFriendChance = VillageBondRecognitionConfig.GetAmbientRecognitionChancePercent(3);

        Assert.Equal(
            oneFriendRoll < VillageBondRecognitionConfig.AmbientRecognitionBaseChancePercent,
            VillageBondRecognitionConfig.ShouldTriggerAmbientRecognition(5, 1, 100, 3));
        Assert.True(threeFriendChance > VillageBondRecognitionConfig.AmbientRecognitionBaseChancePercent);
    }

    [Fact]
    public void GetAmbientRecognitionChancePercent_IsHigherForMultipleCloseFriends()
    {
        var single = VillageBondRecognitionConfig.GetAmbientRecognitionChancePercent(1);
        var multi = VillageBondRecognitionConfig.GetAmbientRecognitionChancePercent(2);

        Assert.True(multi > single);
        Assert.True(multi >= VillageBondRecognitionConfig.MultiBondAmbientBaseChancePercent);
    }

    [Fact]
    public void GetPassiveMoodRecoveryPerGameMinute_RequiresTwoCloseFriends()
    {
        Assert.Equal(0f, VillageBondRecognitionConfig.GetPassiveMoodRecoveryPerGameMinute(1, false));
        Assert.Equal(
            VillageBondRecognitionConfig.PassiveMoodRecoveryPerGameMinute,
            VillageBondRecognitionConfig.GetPassiveMoodRecoveryPerGameMinute(2, false));
        Assert.True(
            VillageBondRecognitionConfig.GetPassiveMoodRecoveryPerGameMinute(2, true)
            > VillageBondRecognitionConfig.GetPassiveMoodRecoveryPerGameMinute(2, false));
    }

    [Fact]
    public void TryGetVillageAmbientComment_ReturnsNaturalLineForSingleBond()
    {
        var comment = VillageBondRecognitionConfig.TryGetVillageAmbientComment(
            [NpcEntityIds.Elsie],
            villageNoticedMemory: false,
            variationSeed: 0);

        Assert.NotNull(comment);
        Assert.Contains("Elsie", comment, StringComparison.Ordinal);
        Assert.DoesNotContain("emotional bond", comment, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetVillageAmbientComment_ReturnsWarmerLineForMultipleBonds()
    {
        var comment = VillageBondRecognitionConfig.TryGetVillageAmbientComment(
            [NpcEntityIds.Elsie, NpcEntityIds.Tom],
            villageNoticedMemory: false,
            variationSeed: 0);

        Assert.NotNull(comment);
        Assert.Contains("circle", comment, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetCrossNpcRecognitionLine_ReturnsWarmCrossReference()
    {
        var line = VillageBondRecognitionConfig.TryGetCrossNpcRecognitionLine(
            NpcEntityIds.Elsie,
            [NpcEntityIds.Elsie, NpcEntityIds.Tom],
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("Tom", line, StringComparison.Ordinal);
    }

    [Fact]
    public void TryGetVillageAppreciationLine_ReturnsPersonalAppreciation()
    {
        var line = VillageBondRecognitionConfig.TryGetVillageAppreciationLine(
            NpcEntityIds.Elsie,
            focusCloseFriendCount: 2,
            villageNoticedMemory: true,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.True(
            line.Contains("Bloomtown", StringComparison.OrdinalIgnoreCase)
            || line.Contains("friends", StringComparison.OrdinalIgnoreCase)
            || line.Contains("village", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void FormatRecognitionStatus_ShowsWarmerToneForMultipleBonds()
    {
        var single = VillageBondRecognitionConfig.FormatRecognitionStatus(
            [NpcEntityIds.Elsie],
            villageNoticedMemory: false);
        var multi = VillageBondRecognitionConfig.FormatRecognitionStatus(
            [NpcEntityIds.Elsie, NpcEntityIds.Tom],
            villageNoticedMemory: true,
            passiveBenefitActive: true);

        Assert.NotNull(single);
        Assert.Contains("starting to notice", single, StringComparison.OrdinalIgnoreCase);
        Assert.NotNull(multi);
        Assert.Contains("regard", multi, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("warmth", multi, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatAmbientRecognitionFeedback_IsClearAndPositive()
    {
        var feedback = VillageBondRecognitionConfig.FormatAmbientRecognitionFeedback([NpcEntityIds.Harold]);

        Assert.StartsWith("[The village has noticed", feedback, StringComparison.Ordinal);
        Assert.Contains("Harold", feedback, StringComparison.Ordinal);
    }

    [Fact]
    public void ShouldRecordVillageMemory_RequiresTwoFocusCloseFriends()
    {
        Assert.False(VillageBondRecognitionConfig.ShouldRecordVillageMemory(1));
        Assert.True(VillageBondRecognitionConfig.ShouldRecordVillageMemory(2));
    }

    [Fact]
    public void VillageNoticedYourBonds_IsVillageWideMemory()
    {
        Assert.True(NpcMemoryConfig.IsVillageWideMemory(NpcMemoryType.VillageNoticedYourBonds));
        Assert.Equal(
            NpcMemoryConfig.VillageWideNpcEntityId,
            NpcMemoryConfig.GetStorageNpcEntityId(5, NpcMemoryType.VillageNoticedYourBonds));
    }
}