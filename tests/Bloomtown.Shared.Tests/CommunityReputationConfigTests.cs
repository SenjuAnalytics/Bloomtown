using Bloomtown.Shared.Community;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Tests;

public sealed class CommunityReputationConfigTests
{
    [Fact]
    public void Increment_IncreasesCorrectActivityCounter()
    {
        var state = CommunityReputationConfig.CreateEmpty();
        var afterGarden = CommunityReputationConfig.Increment(state, CommunityActivityKind.HelpGarden);
        var afterMarket = CommunityReputationConfig.Increment(afterGarden, CommunityActivityKind.HelpMarket);

        Assert.Equal(1, afterGarden.HelpGardenCount);
        Assert.Equal(1, afterMarket.HelpMarketCount);
        Assert.Equal(2, afterMarket.TotalHelpCount);
    }

    [Fact]
    public void ResolveSocialRole_ReturnsGardenHelperWhenDominant()
    {
        var state = new CommunityReputationState(HelpGardenCount: 6, HelpMarketCount: 1, HelpWellCount: 0);
        Assert.Equal(CommunitySocialRole.GardenHelper, CommunityReputationConfig.ResolveSocialRole(state));
    }

    [Fact]
    public void ResolveSocialRole_ReturnsAllRoundHelperForBalancedHighTotals()
    {
        var state = new CommunityReputationState(HelpGardenCount: 4, HelpMarketCount: 4, HelpWellCount: 4);
        Assert.Equal(CommunitySocialRole.AllRoundHelper, CommunityReputationConfig.ResolveSocialRole(state));
    }

    [Fact]
    public void TryGetRecurringHelpAcknowledgment_RequiresMinimumHelps()
    {
        var low = new CommunityReputationState(2, 0, 0);
        var high = new CommunityReputationState(4, 0, 0);

        Assert.Null(CommunityReputationConfig.TryGetRecurringHelpAcknowledgment(
            CommunityActivityKind.HelpGarden,
            low,
            NpcEntityIds.Elsie,
            variationSeed: 0));

        Assert.False(string.IsNullOrWhiteSpace(CommunityReputationConfig.TryGetRecurringHelpAcknowledgment(
            CommunityActivityKind.HelpGarden,
            high,
            NpcEntityIds.Elsie,
            variationSeed: 0)));
    }

    [Fact]
    public void FormatParticipationStatus_ShowsSocialRole()
    {
        var state = new CommunityReputationState(HelpGardenCount: 6, HelpMarketCount: 1, HelpWellCount: 0);
        var status = CommunityReputationConfig.FormatParticipationStatus(state);

        Assert.Contains("regular garden helper", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatParticipationStatus_ShowsEarlyParticipation()
    {
        var state = new CommunityReputationState(HelpGardenCount: 1, HelpMarketCount: 1, HelpWellCount: 1);
        var status = CommunityReputationConfig.FormatParticipationStatus(state);

        Assert.Contains("3 help session", status, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(CommunityActivityKind.HelpGarden, NpcEntityIds.Elsie)]
    [InlineData(CommunityActivityKind.HelpMarket, NpcEntityIds.Tom)]
    [InlineData(CommunityActivityKind.HelpWell, NpcEntityIds.Elsie)]
    public void TryGetInteractionRecognition_ReturnsElsieOrTomLine(
        CommunityActivityKind activity,
        uint npcEntityId)
    {
        var state = new CommunityReputationState(
            activity == CommunityActivityKind.HelpGarden ? 6 : 0,
            activity == CommunityActivityKind.HelpMarket ? 6 : 0,
            activity == CommunityActivityKind.HelpWell ? 6 : 0);

        var line = CommunityReputationConfig.TryGetInteractionRecognition(state, npcEntityId, variationSeed: 1);
        Assert.False(string.IsNullOrWhiteSpace(line));
    }

    [Fact]
    public void ShouldTriggerRecurringHelpAcknowledgment_UsesConfiguredThreshold()
    {
        var roll = (5u * 61 + (uint)(100 % 971)) % 100;
        Assert.Equal(roll < CommunityReputationConfig.RecurringHelpAcknowledgmentChancePercent,
            CommunityReputationConfig.ShouldTriggerRecurringHelpAcknowledgment(5, 100));
    }

    [Fact]
    public void GetDominantSocialRole_MatchesResolveSocialRole()
    {
        var state = new CommunityReputationState(HelpGardenCount: 6, HelpMarketCount: 1, HelpWellCount: 0);
        Assert.Equal(
            CommunityReputationConfig.ResolveSocialRole(state),
            CommunityReputationConfig.GetDominantSocialRole(state));
    }

    [Theory]
    [InlineData(CommunitySocialRole.GardenHelper, CommunityActivityKind.HelpGarden, true)]
    [InlineData(CommunitySocialRole.GardenHelper, CommunityActivityKind.HelpMarket, false)]
    [InlineData(CommunitySocialRole.MarketHelper, CommunityActivityKind.HelpMarket, true)]
    [InlineData(CommunitySocialRole.WellKeeper, CommunityActivityKind.HelpWell, true)]
    [InlineData(CommunitySocialRole.AllRoundHelper, CommunityActivityKind.HelpGarden, true)]
    [InlineData(CommunitySocialRole.AllRoundHelper, CommunityActivityKind.HelpWell, true)]
    [InlineData(CommunitySocialRole.None, CommunityActivityKind.HelpGarden, false)]
    public void IsActivityAlignedWithSocialRole_MatchesRoleToArea(
        CommunitySocialRole role,
        CommunityActivityKind activity,
        bool expected)
    {
        Assert.Equal(
            expected,
            CommunityReputationConfig.IsActivityAlignedWithSocialRole(role, activity));
    }

    [Fact]
    public void GetConsistentHelperEffectBonus_ReturnsBonusForAlignedRole()
    {
        var state = new CommunityReputationState(HelpGardenCount: 6, HelpMarketCount: 1, HelpWellCount: 0);
        var (moodBonus, socialBonus) = CommunityReputationConfig.GetConsistentHelperEffectBonus(
            state,
            CommunityActivityKind.HelpGarden);

        Assert.Equal(CommunityReputationConfig.ConsistentHelperMoodBonus, moodBonus);
        Assert.Equal(CommunityReputationConfig.ConsistentHelperSocialBonus, socialBonus);
    }

    [Fact]
    public void GetConsistentHelperEffectBonus_ReturnsZeroForMisalignedActivity()
    {
        var state = new CommunityReputationState(HelpGardenCount: 6, HelpMarketCount: 1, HelpWellCount: 0);
        var (moodBonus, socialBonus) = CommunityReputationConfig.GetConsistentHelperEffectBonus(
            state,
            CommunityActivityKind.HelpMarket);

        Assert.Equal(0f, moodBonus);
        Assert.Equal(0f, socialBonus);
    }

    [Theory]
    [InlineData(3, 0, 0, true)]
    [InlineData(2, 0, 0, false)]
    [InlineData(6, 1, 0, true)]
    public void IsEligibleForVillageReaction_RequiresParticipationOrRole(
        int garden,
        int market,
        int well,
        bool expected)
    {
        var state = new CommunityReputationState(garden, market, well);
        Assert.Equal(expected, CommunityReputationConfig.IsEligibleForVillageReaction(state));
    }

    [Fact]
    public void IsRightMomentForVillageReaction_RequiresRoleForInteraction()
    {
        var frequentNoRole = new CommunityReputationState(2, 1, 0);
        var gardenHelper = new CommunityReputationState(6, 1, 0);

        Assert.True(CommunityReputationConfig.IsRightMomentForVillageReaction(
            frequentNoRole,
            VillageReactionSurface.RecurringHelpAcknowledgment));
        Assert.False(CommunityReputationConfig.IsRightMomentForVillageReaction(
            frequentNoRole,
            VillageReactionSurface.InteractionRecognition));
        Assert.True(CommunityReputationConfig.IsRightMomentForVillageReaction(
            gardenHelper,
            VillageReactionSurface.InteractionRecognition));
    }

    [Fact]
    public void FormatParticipationStatus_ShowsVillageViewForRole()
    {
        var state = new CommunityReputationState(HelpGardenCount: 6, HelpMarketCount: 1, HelpWellCount: 0);
        var status = CommunityReputationConfig.FormatParticipationStatus(state);

        Assert.Contains("count on you around the garden", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetRecurringHelpAcknowledgment_UsesFamiliarPresenceWhenNoRole()
    {
        var state = new CommunityReputationState(HelpGardenCount: 4, HelpMarketCount: 0, HelpWellCount: 0);
        var line = CommunityReputationConfig.TryGetRecurringHelpAcknowledgment(
            CommunityActivityKind.HelpGarden,
            state,
            NpcEntityIds.Elsie,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains("used to", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetRecurringHelpAcknowledgment_UsesDependenceLineWhenRoleEarned()
    {
        var state = new CommunityReputationState(HelpGardenCount: 6, HelpMarketCount: 1, HelpWellCount: 0);
        var line = CommunityReputationConfig.TryGetRecurringHelpAcknowledgment(
            CommunityActivityKind.HelpGarden,
            state,
            NpcEntityIds.Elsie,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains("count on", line, StringComparison.OrdinalIgnoreCase);
    }
}