using Bloomtown.Shared.Community;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Goals;

namespace Bloomtown.Shared.Tests;

public sealed class LegacyArchetypeConfigTests
{
    [Fact]
    public void ResolveDominantArchetype_ReturnsNoneForNewPlayers()
    {
        var snapshot = Snapshot();
        var reputation = CommunityReputationConfig.CreateEmpty();

        Assert.Equal(LegacyArchetype.None, LegacyArchetypeConfig.ResolveDominantArchetype(snapshot, reputation));
    }

    [Fact]
    public void ResolveDominantArchetype_DetectsBuilderFromContribution()
    {
        var snapshot = Snapshot(contribution: 80, projects: 2, title: VillageTitle.Builder);
        var reputation = new CommunityReputationState(1, 0, 0);

        Assert.Equal(LegacyArchetype.Builder, LegacyArchetypeConfig.ResolveDominantArchetype(snapshot, reputation));
    }

    [Fact]
    public void ResolveDominantArchetype_DetectsCaretakerFromHelpsAndRole()
    {
        var snapshot = Snapshot(helps: 8, role: CommunitySocialRole.GardenHelper);
        var reputation = new CommunityReputationState(HelpGardenCount: 8, HelpMarketCount: 0, HelpWellCount: 0);

        Assert.Equal(LegacyArchetype.Caretaker, LegacyArchetypeConfig.ResolveDominantArchetype(snapshot, reputation));
    }

    [Fact]
    public void ResolveDominantArchetype_DetectsConnectorFromFriendships()
    {
        var snapshot = Snapshot(friends: 2, acquaintances: 3, closeFriends: 1, role: CommunitySocialRole.MarketHelper);
        var reputation = new CommunityReputationState(1, 1, 0);

        Assert.Equal(LegacyArchetype.Connector, LegacyArchetypeConfig.ResolveDominantArchetype(snapshot, reputation));
    }

    [Fact]
    public void FormatLegacyPathStatusLine_ReturnsCaretakerFlavor()
    {
        var line = LegacyArchetypeConfig.FormatLegacyPathStatusLine(LegacyArchetype.Caretaker);

        Assert.NotNull(line);
        Assert.Contains("Village identity", line, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rely on", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetMilestoneFeedbackLine_DiffersByArchetype()
    {
        var builder = PlayerLongTermGoalConfig.TryGetMilestoneFeedbackLine(
            PlayerLongTermGoalMilestone.VillageStory,
            LegacyArchetype.Builder,
            variationSeed: 0);
        var connector = PlayerLongTermGoalConfig.TryGetMilestoneFeedbackLine(
            PlayerLongTermGoalMilestone.VillageStory,
            LegacyArchetype.Connector,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(builder));
        Assert.False(string.IsNullOrWhiteSpace(connector));
        Assert.NotEqual(builder, connector);
    }

    private static PlayerLongTermGoalSnapshot Snapshot(
        int helps = 0,
        CommunitySocialRole role = CommunitySocialRole.None,
        VillageTitle title = VillageTitle.Newcomer,
        int contribution = 0,
        int friends = 0,
        int acquaintances = 0,
        int closeFriends = 0,
        int projects = 0) =>
        new(
            helps,
            role,
            title,
            contribution,
            friends,
            acquaintances,
            closeFriends,
            projects,
            HasLegacyRecognition: false);
}