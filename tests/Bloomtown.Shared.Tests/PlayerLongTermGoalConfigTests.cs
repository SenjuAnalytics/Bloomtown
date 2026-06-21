using Bloomtown.Shared.Community;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Goals;

namespace Bloomtown.Shared.Tests;

public sealed class PlayerLongTermGoalConfigTests
{
    [Fact]
    public void IsMilestoneMet_PuttingDownRoots_RequiresHelps()
    {
        var low = Snapshot(helps: 2);
        var enough = Snapshot(helps: 3);

        Assert.False(PlayerLongTermGoalConfig.IsMilestoneMet(
            PlayerLongTermGoalMilestone.PuttingDownRoots,
            low));
        Assert.True(PlayerLongTermGoalConfig.IsMilestoneMet(
            PlayerLongTermGoalMilestone.PuttingDownRoots,
            enough));
    }

    [Fact]
    public void IsMilestoneMet_TrustedNeighbor_RequiresSocialRole()
    {
        var none = Snapshot(helps: 6, title: VillageTitle.Helper, friends: 1, acquaintances: 1, projects: 1, legacy: true);
        var role = none with { SocialRole = CommunitySocialRole.GardenHelper };

        Assert.False(PlayerLongTermGoalConfig.IsMilestoneMet(
            PlayerLongTermGoalMilestone.TrustedNeighbor,
            none));
        Assert.True(PlayerLongTermGoalConfig.IsMilestoneMet(
            PlayerLongTermGoalMilestone.TrustedNeighbor,
            role));
    }

    [Fact]
    public void IsMilestoneMet_VillageStory_AcceptsHelperTitleOrFriendOrProject()
    {
        var helper = Snapshot(helps: 6, role: CommunitySocialRole.GardenHelper, title: VillageTitle.Helper, acquaintances: 1);
        var friend = helper with { VillageTitle = VillageTitle.Newcomer, FriendCount = 1 };
        var project = helper with { VillageTitle = VillageTitle.Newcomer, CompletedProjectContributions = 1 };

        Assert.True(PlayerLongTermGoalConfig.IsMilestoneMet(PlayerLongTermGoalMilestone.VillageStory, helper));
        Assert.True(PlayerLongTermGoalConfig.IsMilestoneMet(PlayerLongTermGoalMilestone.VillageStory, friend));
        Assert.True(PlayerLongTermGoalConfig.IsMilestoneMet(PlayerLongTermGoalMilestone.VillageStory, project));
    }

    [Fact]
    public void IsMilestoneMet_BloomtownLegacy_AcceptsBuilderTitleOrRecognitionOrCloseFriend()
    {
        var builder = Snapshot(helps: 10, role: CommunitySocialRole.GardenHelper, title: VillageTitle.Builder, friends: 1, acquaintances: 1, projects: 1);
        var legacy = builder with { VillageTitle = VillageTitle.Helper, HasLegacyRecognition = true };
        var close = builder with { VillageTitle = VillageTitle.Helper, CloseFriendCount = 1 };

        Assert.True(PlayerLongTermGoalConfig.IsMilestoneMet(PlayerLongTermGoalMilestone.BloomtownLegacy, builder));
        Assert.True(PlayerLongTermGoalConfig.IsMilestoneMet(PlayerLongTermGoalMilestone.BloomtownLegacy, legacy));
        Assert.True(PlayerLongTermGoalConfig.IsMilestoneMet(PlayerLongTermGoalMilestone.BloomtownLegacy, close));
    }

    [Fact]
    public void EvaluateNewMilestones_AdvancesInOrder()
    {
        var progress = PlayerLongTermGoalConfig.CreateDefault();
        var snapshot = Snapshot(
            helps: 6,
            role: CommunitySocialRole.GardenHelper,
            title: VillageTitle.Helper,
            friends: 1,
            acquaintances: 2,
            projects: 1);

        var completed = PlayerLongTermGoalConfig.EvaluateNewMilestones(progress, snapshot);

        Assert.Equal(3, completed.Count);
        Assert.Equal(PlayerLongTermGoalMilestone.PuttingDownRoots, completed[0]);
        Assert.Equal(PlayerLongTermGoalMilestone.TrustedNeighbor, completed[1]);
        Assert.Equal(PlayerLongTermGoalMilestone.VillageStory, completed[2]);
    }

    [Fact]
    public void FormatGoalStatusLine_ShowsNextMilestoneHint()
    {
        var progress = PlayerLongTermGoalConfig.CreateDefault();
        var snapshot = Snapshot(helps: 1);

        var status = PlayerLongTermGoalConfig.FormatGoalStatusLine(progress, snapshot);

        Assert.Contains("Putting Down Roots", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("1/3 helps", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatGoalStatusLine_IncludesLegacyPathWhenDetected()
    {
        var progress = new PlayerLongTermGoalProgress(
            PlayerLongTermGoalKind.VillageLegacy,
            PlayerLongTermGoalMilestone.PuttingDownRoots,
            null,
            LegacyArchetype.Caretaker);
        var snapshot = Snapshot(helps: 6, role: CommunitySocialRole.GardenHelper);

        var status = PlayerLongTermGoalConfig.FormatGoalStatusLine(progress, snapshot);

        Assert.Contains("Village identity", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatGoalDetail_ListsMilestones()
    {
        var progress = PlayerLongTermGoalConfig.CreateDefault();
        var snapshot = Snapshot();

        var detail = PlayerLongTermGoalConfig.FormatGoalDetail(progress, snapshot);

        Assert.Contains("Building Your Legacy in Bloomtown", detail, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Putting Down Roots", detail, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Bloomtown Legacy", detail, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetMilestoneFeedbackLine_ReturnsArchetypeFlavor()
    {
        var line = PlayerLongTermGoalConfig.TryGetMilestoneFeedbackLine(
            PlayerLongTermGoalMilestone.BloomtownLegacy,
            LegacyArchetype.Builder,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains("builder", line, StringComparison.OrdinalIgnoreCase);
    }

    private static PlayerLongTermGoalSnapshot Snapshot(
        int helps = 0,
        CommunitySocialRole role = CommunitySocialRole.None,
        VillageTitle title = VillageTitle.Newcomer,
        int contribution = 0,
        int friends = 0,
        int acquaintances = 0,
        int closeFriends = 0,
        int projects = 0,
        bool legacy = false) =>
        new(
            helps,
            role,
            title,
            contribution,
            friends,
            acquaintances,
            closeFriends,
            projects,
            legacy);
}