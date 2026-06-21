using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Housing;

namespace Bloomtown.Shared.Tests;

public sealed class PlayerMilestoneConfigTests
{
    [Fact]
    public void IsMilestoneMet_FirstFurnishing_RequiresPlacedFurniture()
    {
        var progress = PlayerMilestoneConfig.CreateDefault();
        var empty = Snapshot(furniture: 0);
        var furnished = Snapshot(furniture: 1);

        Assert.False(PlayerMilestoneConfig.IsMilestoneMet(PlayerMilestoneKind.FirstFurnishing, empty, progress));
        Assert.True(PlayerMilestoneConfig.IsMilestoneMet(PlayerMilestoneKind.FirstFurnishing, furnished, progress));
    }

    [Fact]
    public void IsMilestoneMet_ComfortableNest_RequiresMediumComfort()
    {
        var progress = PlayerMilestoneConfig.CreateDefault();
        var low = Snapshot(comfort: FurnitureComfortConfig.MediumComfortThreshold - 1);
        var enough = Snapshot(comfort: FurnitureComfortConfig.MediumComfortThreshold);

        Assert.False(PlayerMilestoneConfig.IsMilestoneMet(PlayerMilestoneKind.ComfortableNest, low, progress));
        Assert.True(PlayerMilestoneConfig.IsMilestoneMet(PlayerMilestoneKind.ComfortableNest, enough, progress));
    }

    [Fact]
    public void IsMilestoneMet_HelpingHand_RequiresThreeHelps()
    {
        var progress = PlayerMilestoneConfig.CreateDefault();
        var low = Snapshot(helps: 2);
        var enough = Snapshot(helps: 3);

        Assert.False(PlayerMilestoneConfig.IsMilestoneMet(PlayerMilestoneKind.HelpingHand, low, progress));
        Assert.True(PlayerMilestoneConfig.IsMilestoneMet(PlayerMilestoneKind.HelpingHand, enough, progress));
    }

    [Fact]
    public void IsMilestoneMet_SteadyRhythm_RequiresThreeAgencyDays()
    {
        var progress = PlayerMilestoneConfig.CreateDefault();
        progress.RhythmAgencyDays.Add(1);
        progress.RhythmAgencyDays.Add(2);

        var snapshot = Snapshot();
        Assert.False(PlayerMilestoneConfig.IsMilestoneMet(PlayerMilestoneKind.SteadyRhythm, snapshot, progress));

        progress.RhythmAgencyDays.Add(3);
        Assert.True(PlayerMilestoneConfig.IsMilestoneMet(PlayerMilestoneKind.SteadyRhythm, snapshot, progress));
    }

    [Fact]
    public void IsMilestoneMet_VillageRegular_RequiresActivitiesAcrossDays()
    {
        var progress = PlayerMilestoneConfig.CreateDefault();
        progress.DailyActivityCount = 4;
        progress.DailyActivityDays.Add(1);
        progress.DailyActivityDays.Add(2);

        var snapshot = Snapshot();
        Assert.False(PlayerMilestoneConfig.IsMilestoneMet(PlayerMilestoneKind.VillageRegular, snapshot, progress));

        progress.DailyActivityDays.Add(3);
        Assert.True(PlayerMilestoneConfig.IsMilestoneMet(PlayerMilestoneKind.VillageRegular, snapshot, progress));
    }

    [Fact]
    public void EvaluateNewMilestones_SkipsAlreadyCompleted()
    {
        var progress = PlayerMilestoneConfig.CreateDefault();
        progress.Completed.Add(PlayerMilestoneKind.FirstFurnishing);

        var snapshot = Snapshot(furniture: 1, comfort: 25, helps: 3, closeFriends: 2);
        var completed = PlayerMilestoneConfig.EvaluateNewMilestones(progress, snapshot);

        Assert.DoesNotContain(PlayerMilestoneKind.FirstFurnishing, completed);
        Assert.Contains(PlayerMilestoneKind.ComfortableNest, completed);
        Assert.Contains(PlayerMilestoneKind.HelpingHand, completed);
        Assert.Contains(PlayerMilestoneKind.CloseFriend, completed);
        Assert.Contains(PlayerMilestoneKind.RespectedNeighbor, completed);
    }

    [Fact]
    public void FormatStatusLine_ShowsNextMilestoneProgress()
    {
        var progress = PlayerMilestoneConfig.CreateDefault();
        progress.Completed.Add(PlayerMilestoneKind.FirstFurnishing);
        var snapshot = Snapshot(furniture: 1, helps: 1);

        var status = PlayerMilestoneConfig.FormatStatusLine(progress, snapshot);

        Assert.Contains("1/7", status);
        Assert.Contains("Comfortable Nest", status);
        Assert.Contains("comfort", status);
    }

    [Fact]
    public void FormatStatusLine_ShowsCompleteMessage()
    {
        var progress = PlayerMilestoneConfig.CreateDefault();
        foreach (var kind in PlayerMilestoneConfig.AllMilestones)
            progress.Completed.Add(kind);

        var status = PlayerMilestoneConfig.FormatStatusLine(progress, Snapshot());

        Assert.Contains("7/7", status);
        Assert.Contains("rhythm", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetCompletionFeedback_ReturnsMessageForEachMilestone()
    {
        foreach (var kind in PlayerMilestoneConfig.AllMilestones)
        {
            var feedback = PlayerMilestoneConfig.GetCompletionFeedback(kind);
            Assert.False(string.IsNullOrWhiteSpace(feedback));
            Assert.Contains("Personal milestone", feedback);
        }
    }

    private static PlayerMilestoneSnapshot Snapshot(
        int furniture = 0,
        int comfort = 0,
        int helps = 0,
        int closeFriends = 0,
        int rhythmDays = 0,
        int dailyActivities = 0,
        int dailyActivityDays = 0) =>
        new(
            furniture,
            comfort,
            helps,
            closeFriends,
            rhythmDays,
            dailyActivities,
            dailyActivityDays);
}