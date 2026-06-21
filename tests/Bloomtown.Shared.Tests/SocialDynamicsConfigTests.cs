using Bloomtown.Shared.Community;
using Bloomtown.Shared.Npc;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.Village;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Tests;

public sealed class SocialDynamicsConfigTests
{
    [Theory]
    [InlineData(NpcEntityIds.Elsie, true)]
    [InlineData(NpcEntityIds.Mira, true)]
    [InlineData(NpcEntityIds.Harold, true)]
    [InlineData(NpcEntityIds.Greta, true)]
    [InlineData(NpcEntityIds.Tom, false)]
    public void IsInfoSharingNpc_IncludesInnkeeperAndSocialHubNpcs(uint npcEntityId, bool expected)
    {
        Assert.Equal(expected, SocialDynamicsConfig.IsInfoSharingNpc(npcEntityId));
    }

    [Fact]
    public void GetInteractionBonus_StacksFriendAndSocialRole()
    {
        var (mood, social) = SocialDynamicsConfig.GetInteractionBonus(
            RelationshipTier.Friend,
            CommunitySocialRole.GardenHelper);

        Assert.Equal(
            SocialDynamicsConfig.FriendInteractionMoodBonus + SocialDynamicsConfig.SocialRoleInteractionMoodBonus,
            mood);
        Assert.Equal(
            SocialDynamicsConfig.FriendInteractionSocialBonus + SocialDynamicsConfig.SocialRoleInteractionSocialBonus,
            social);
    }

    [Fact]
    public void QualifiesForBetterTreatment_FriendOrSocialRole()
    {
        Assert.True(SocialDynamicsConfig.QualifiesForBetterTreatment(
            RelationshipTier.Friend,
            CommunitySocialRole.None));
        Assert.True(SocialDynamicsConfig.QualifiesForBetterTreatment(
            RelationshipTier.Acquaintance,
            CommunitySocialRole.MarketHelper));
        Assert.False(SocialDynamicsConfig.QualifiesForBetterTreatment(
            RelationshipTier.Acquaintance,
            CommunitySocialRole.None));
    }

    [Fact]
    public void FormatSocialStanding_ShowsCloseFriends()
    {
        var reputation = CommunityReputationConfig.CreateEmpty();
        var status = SocialDynamicsConfig.FormatSocialStanding(
            friendCount: 1,
            closeFriendCount: 2,
            acquaintanceCount: 0,
            reputation);

        Assert.Contains("Social standing: well known", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("2 close friends", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatSocialStanding_ShowsSocialRole()
    {
        var reputation = new CommunityReputationState(HelpGardenCount: 6, HelpMarketCount: 0, HelpWellCount: 0);
        var status = SocialDynamicsConfig.FormatSocialStanding(
            friendCount: 0,
            closeFriendCount: 0,
            acquaintanceCount: 2,
            reputation);

        Assert.Contains("regular garden helper", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatSocialStanding_ShowsStrangerDefault()
    {
        var reputation = CommunityReputationConfig.CreateEmpty();
        var status = SocialDynamicsConfig.FormatSocialStanding(0, 0, 0, reputation);

        Assert.Contains("still a stranger", status, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(NpcEntityIds.Elsie)]
    [InlineData(NpcEntityIds.Mira)]
    [InlineData(NpcEntityIds.Harold)]
    public void TryGetLightInfoTip_ReturnsFlavorForInfoSharingNpcs(uint npcEntityId)
    {
        var tip = SocialDynamicsConfig.TryGetLightInfoTip(
            npcEntityId,
            GameTimeOfDay.Morning,
            VillageDevelopmentLevel.Lively,
            completedProjectIds: Array.Empty<byte>(),
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(tip));
    }

    [Fact]
    public void IsEligibleForPersonalHabit_RequiresAcquaintanceAndRecognition()
    {
        var lowHelps = new CommunityReputationState(1, 0, 0);
        var enoughHelps = new CommunityReputationState(3, 0, 0);

        Assert.False(SocialDynamicsConfig.IsEligibleForPersonalHabit(
            RelationshipTier.Stranger,
            enoughHelps));
        Assert.True(SocialDynamicsConfig.IsEligibleForPersonalHabit(
            RelationshipTier.Acquaintance,
            enoughHelps));
        Assert.True(SocialDynamicsConfig.IsEligibleForPersonalHabit(
            RelationshipTier.Friend,
            lowHelps));
    }

    [Fact]
    public void TryGetPersonalHabitLine_ReturnsElsieLineWhenEligible()
    {
        var reputation = new CommunityReputationState(HelpGardenCount: 6, HelpMarketCount: 0, HelpWellCount: 0);
        var line = SocialDynamicsConfig.TryGetPersonalHabitLine(
            NpcEntityIds.Elsie,
            RelationshipTier.Friend,
            reputation,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
    }

    [Fact]
    public void TryGetContextualAmbientComment_ReturnsWarmLineForFriend()
    {
        var reputation = new CommunityReputationState(HelpGardenCount: 6, HelpMarketCount: 0, HelpWellCount: 0);
        var comment = SocialDynamicsConfig.TryGetContextualAmbientComment(
            NpcEntityIds.Elsie,
            RelationshipTier.Friend,
            reputation,
            NpcInterpersonalRelationship.Friendly,
            GameTimeOfDay.Afternoon,
            VillageDevelopmentLevel.Lively,
            completedProjectIds: Array.Empty<byte>(),
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(comment));
    }

    [Fact]
    public void ShouldTriggerLightInfo_UsesConfiguredThreshold()
    {
        var roll = (5u * 79 + (uint)(100 % 953)) % 100;
        Assert.Equal(
            roll < SocialDynamicsConfig.LightInfoChancePercent,
            SocialDynamicsConfig.ShouldTriggerLightInfo(5, 100));
    }

    [Fact]
    public void ShouldTriggerPersonalHabit_UsesConfiguredThreshold()
    {
        var roll = (5u * 83 + (uint)(100 % 947)) % 100;
        Assert.Equal(
            roll < SocialDynamicsConfig.PersonalHabitChancePercent,
            SocialDynamicsConfig.ShouldTriggerPersonalHabit(5, 100));
    }

    [Fact]
    public void ShouldTriggerContextualAmbient_UsesConfiguredThreshold()
    {
        var roll = (5u * 89 + (uint)(100 % 941) + 3u * 5) % 100;
        Assert.Equal(
            roll < SocialDynamicsConfig.ContextualAmbientChancePercent,
            SocialDynamicsConfig.ShouldTriggerContextualAmbient(5, 100, 3));
    }
}