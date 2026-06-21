using Bloomtown.Shared.Community;
using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.Tests;

public sealed class CommunityActivityConfigTests
{
    [Fact]
    public void All_DefinesTwelveCommunityActivities()
    {
        Assert.Equal(12, CommunityActivityConfig.All.Count);
    }

    [Fact]
    public void TryGet_MapsCommands()
    {
        Assert.True(CommunityActivityConfig.TryGet(CommunityActivityKind.HelpGarden, out var garden));
        Assert.Equal("help garden", garden.CommandHint);

        Assert.True(CommunityActivityConfig.TryGet(CommunityActivityKind.HelpMarket, out var market));
        Assert.Equal("help market", market.CommandHint);

        Assert.True(CommunityActivityConfig.TryGet(CommunityActivityKind.HelpWell, out var well));
        Assert.Equal("help well", well.CommandHint);

        Assert.True(CommunityActivityConfig.TryGet(CommunityActivityKind.HelpLumber, out var lumber));
        Assert.Equal("help lumber", lumber.CommandHint);

        Assert.True(CommunityActivityConfig.TryGet(CommunityActivityKind.HelpInn, out var inn));
        Assert.Equal("help inn", inn.CommandHint);

        Assert.True(CommunityActivityConfig.TryGet(CommunityActivityKind.HelpHerbGarden, out var herbGarden));
        Assert.Equal("help herb garden", herbGarden.CommandHint);

        Assert.True(CommunityActivityConfig.TryGet(CommunityActivityKind.HelpSmithy, out var smithy));
        Assert.Equal("help smithy", smithy.CommandHint);

        Assert.True(CommunityActivityConfig.TryGet(CommunityActivityKind.HelpWorkshop, out var workshop));
        Assert.Equal("help workshop", workshop.CommandHint);
        Assert.Equal("Village Workshop", workshop.LocationName);
        Assert.Equal(10f, workshop.WorldX);
        Assert.Equal(16f, workshop.WorldZ);

        Assert.True(CommunityActivityConfig.TryGet(CommunityActivityKind.HelpPatrol, out var patrol));
        Assert.Equal("help patrol", patrol.CommandHint);

        Assert.True(CommunityActivityConfig.TryGet(CommunityActivityKind.HelpVillage, out var village));
        Assert.Equal("help village", village.CommandHint);

        Assert.True(CommunityActivityConfig.TryGet(CommunityActivityKind.ListenToStories, out var stories));
        Assert.Equal("listen to stories", stories.CommandHint);
        Assert.Equal("Storyteller's Bench", stories.LocationName);
        Assert.Equal(11f, stories.WorldX);
        Assert.Equal(9f, stories.WorldZ);

        Assert.True(CommunityActivityConfig.TryGet(CommunityActivityKind.ChatWithEleanor, out var eleanor));
        Assert.Equal("chat with eleanor", eleanor.CommandHint);
        Assert.Equal("Eleanor's Porch", eleanor.LocationName);
        Assert.Equal(13f, eleanor.WorldX);
        Assert.Equal(12f, eleanor.WorldZ);
    }

    [Fact]
    public void CommunityActivities_FavorSocialReliefOverAreaLeisure()
    {
        var gardenHelp = CommunityActivityConfig.All.First(a => a.Kind == CommunityActivityKind.HelpGarden);
        var tendPlants = VillageAreaConfig.AllInteractions.First(i => i.Kind == VillageAreaInteractionKind.TendPlants);

        Assert.True(gardenHelp.SocialReduction > tendPlants.SocialReduction);
        Assert.True(gardenHelp.MoodGain >= tendPlants.MoodGain);
    }

    [Theory]
    [InlineData(CommunityActivityKind.HelpGarden)]
    [InlineData(CommunityActivityKind.HelpMarket)]
    [InlineData(CommunityActivityKind.HelpWell)]
    public void GetCooldown_ReturnsPositiveDuration(CommunityActivityKind kind)
    {
        Assert.True(CommunityActivityConfig.GetCooldown(kind) > TimeSpan.Zero);
    }

    [Fact]
    public void IsAvailableAt_RequiresUnlockAndProximity()
    {
        var garden = CommunityActivityConfig.All.First(a => a.Kind == CommunityActivityKind.HelpGarden);
        var unlocked = new HashSet<VillageArea> { VillageArea.CommunityGarden };
        var completed = new HashSet<byte>();

        Assert.False(CommunityActivityConfig.IsAvailableAt(
            garden,
            playerX: 0f,
            playerZ: 0f,
            unlocked,
            completed));

        Assert.True(CommunityActivityConfig.IsAvailableAt(
            garden,
            garden.WorldX,
            garden.WorldZ,
            unlocked,
            completed));
    }

    [Fact]
    public void HelpWell_RequiresCompletedWellProject()
    {
        var well = CommunityActivityConfig.All.First(a => a.Kind == CommunityActivityKind.HelpWell);
        var unlocked = new HashSet<VillageArea>();
        var noProjects = new HashSet<byte>();
        var withWell = new HashSet<byte> { VillageSiteIds.Well };

        Assert.False(CommunityActivityConfig.MeetsPrerequisites(well, unlocked, noProjects));
        Assert.True(CommunityActivityConfig.MeetsPrerequisites(well, unlocked, withWell));
    }

    [Fact]
    public void FormatNearbyStatus_ListsCommandsWhenInRange()
    {
        var garden = CommunityActivityConfig.All.First(a => a.Kind == CommunityActivityKind.HelpGarden);
        var unlocked = new HashSet<VillageArea> { VillageArea.CommunityGarden };
        var completed = new HashSet<byte>();

        var status = CommunityActivityConfig.FormatNearbyStatus(
            garden.WorldX,
            garden.WorldZ,
            unlocked,
            completed);

        Assert.Contains("help garden", status);
    }

    [Fact]
    public void PickFlavorAndContribution_ReturnNonEmptyLines()
    {
        var activity = CommunityActivityConfig.All[0];
        Assert.False(string.IsNullOrWhiteSpace(CommunityActivityConfig.PickFlavorText(activity, 0)));
        Assert.False(string.IsNullOrWhiteSpace(CommunityActivityConfig.PickContributionText(activity, 0)));
    }

    [Theory]
    [InlineData(CommunityActivityKind.HelpGarden)]
    [InlineData(CommunityActivityKind.HelpMarket)]
    [InlineData(CommunityActivityKind.HelpWell)]
    [InlineData(CommunityActivityKind.HelpInn)]
    [InlineData(CommunityActivityKind.HelpHerbGarden)]
    [InlineData(CommunityActivityKind.HelpSmithy)]
    [InlineData(CommunityActivityKind.HelpWorkshop)]
    [InlineData(CommunityActivityKind.HelpPatrol)]
    [InlineData(CommunityActivityKind.HelpVillage)]
    [InlineData(CommunityActivityKind.ListenToStories)]
    [InlineData(CommunityActivityKind.ChatWithEleanor)]
    public void PickNpcAcknowledgment_ReturnsNonEmptyLine(CommunityActivityKind kind)
    {
        var ack = CommunityActivityDialogue.PickNpcAcknowledgment(kind, 0);
        Assert.False(string.IsNullOrWhiteSpace(ack));
    }
}