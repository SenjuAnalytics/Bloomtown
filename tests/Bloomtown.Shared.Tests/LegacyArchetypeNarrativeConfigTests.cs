using Bloomtown.Shared.Community;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Goals;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Tests;

public sealed class LegacyArchetypeNarrativeConfigTests
{
    [Fact]
    public void FormatVillageIdentityStatusLine_CaretakerFeelsPersonal()
    {
        var line = LegacyArchetypeConfig.FormatVillageIdentityStatusLine(LegacyArchetype.Caretaker);

        Assert.NotNull(line);
        Assert.Contains("rely on", line, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Village identity", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatVillagePerspectiveLine_DiffersByArchetype()
    {
        var builder = LegacyArchetypeConfig.FormatVillagePerspectiveLine(LegacyArchetype.Builder);
        var connector = LegacyArchetypeConfig.FormatVillagePerspectiveLine(LegacyArchetype.Connector);

        Assert.NotNull(builder);
        Assert.NotNull(connector);
        Assert.NotEqual(builder, connector);
    }

    [Fact]
    public void TryGetNpcArchetypeRecognitionLine_DiffersByNpcAndArchetype()
    {
        var elsieBuilder = PlayerLongTermGoalConfig.TryGetNpcArchetypeRecognitionLine(
            NpcEntityIds.Elsie,
            LegacyArchetype.Builder,
            variationSeed: 0);
        var haroldCaretaker = PlayerLongTermGoalConfig.TryGetNpcArchetypeRecognitionLine(
            NpcEntityIds.Harold,
            LegacyArchetype.Caretaker,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(elsieBuilder));
        Assert.False(string.IsNullOrWhiteSpace(haroldCaretaker));
        Assert.NotEqual(elsieBuilder, haroldCaretaker);
    }

    [Fact]
    public void TryGetPersonalAlignedActionFeedback_ReturnsArchetypeSpecificLine()
    {
        var line = PlayerLongTermGoalConfig.TryGetPersonalAlignedActionFeedback(
            LegacyArchetype.Connector,
            LegacyAlignedActionKind.NpcInteraction,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains("connector", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetNarrativeDirectionHint_UsesPersonalPhrasing()
    {
        var influence = new LegacyArchetypeInfluence(0, 5, 0);
        var hint = PlayerLongTermGoalConfig.TryGetNarrativeDirectionHint(
            LegacyArchetype.None,
            influence,
            variationSeed: 0);

        Assert.NotNull(hint);
        Assert.DoesNotContain("Legacy leaning", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatGoalStatusLine_IncludesVillageIdentityWhenDetected()
    {
        var progress = new PlayerLongTermGoalProgress(
            PlayerLongTermGoalKind.VillageLegacy,
            PlayerLongTermGoalMilestone.PuttingDownRoots,
            null,
            LegacyArchetype.Caretaker);
        var snapshot = new PlayerLongTermGoalSnapshot(
            TotalHelpCount: 6,
            SocialRole: CommunitySocialRole.GardenHelper,
            VillageTitle: VillageTitle.Newcomer,
            VillageContributionScore: 0,
            FriendCount: 0,
            AcquaintanceCount: 0,
            CloseFriendCount: 0,
            CompletedProjectContributions: 0,
            HasLegacyRecognition: false);

        var status = PlayerLongTermGoalConfig.FormatGoalStatusLine(progress, snapshot);

        Assert.Contains("Village identity", status, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("rely on", status, StringComparison.OrdinalIgnoreCase);
    }
}