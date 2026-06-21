using Bloomtown.Shared.Community;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Goals;

namespace Bloomtown.Shared.Tests;

public sealed class LegacyArchetypeAgencyConfigTests
{
    [Fact]
    public void ApplyInfluenceGain_IncrementsAndCaps()
    {
        var current = new LegacyArchetypeInfluence(0, LegacyArchetypeAgencyConfig.MaxInfluencePerPath - 1, 0);
        var updated = LegacyArchetypeAgencyConfig.ApplyInfluenceGain(current, LegacyArchetype.Caretaker);

        Assert.Equal(LegacyArchetypeAgencyConfig.MaxInfluencePerPath, updated.CaretakerPoints);
    }

    [Fact]
    public void GetInfluenceForCommunityActivity_MapsGardenToCaretakerAndMarketToConnector()
    {
        Assert.Equal(LegacyArchetype.Caretaker, LegacyArchetypeAgencyConfig.GetInfluenceForCommunityActivity(CommunityActivityKind.HelpGarden));
        Assert.Equal(LegacyArchetype.Connector, LegacyArchetypeAgencyConfig.GetInfluenceForCommunityActivity(CommunityActivityKind.HelpMarket));
    }

    [Fact]
    public void ResolveDominantArchetype_InfluenceCanTipCaretakerPath()
    {
        var snapshot = new PlayerLongTermGoalSnapshot(
            TotalHelpCount: 3,
            SocialRole: CommunitySocialRole.None,
            VillageTitle: VillageTitle.Newcomer,
            VillageContributionScore: 0,
            FriendCount: 0,
            AcquaintanceCount: 0,
            CloseFriendCount: 0,
            CompletedProjectContributions: 0,
            HasLegacyRecognition: false);
        var reputation = new CommunityReputationState(2, 1, 0);
        var influence = new LegacyArchetypeInfluence(0, 6, 0);

        Assert.Equal(
            LegacyArchetype.Caretaker,
            LegacyArchetypeConfig.ResolveDominantArchetype(snapshot, reputation, influence));
    }

    [Fact]
    public void QualifiesForSpecialAction_RequiresInfluenceOrDetectedArchetype()
    {
        var influence = new LegacyArchetypeInfluence(0, 3, 0);

        Assert.False(LegacyArchetypeAgencyConfig.QualifiesForSpecialAction(
            LegacyArchetype.Caretaker,
            LegacyArchetype.None,
            influence));
        Assert.True(LegacyArchetypeAgencyConfig.QualifiesForSpecialAction(
            LegacyArchetype.Caretaker,
            LegacyArchetype.Caretaker,
            influence));
    }

    [Fact]
    public void FormatLegacyDirectionHint_ShowsLeaningPath()
    {
        var influence = new LegacyArchetypeInfluence(0, 5, 0);
        var hint = LegacyArchetypeAgencyConfig.FormatLegacyDirectionHint(LegacyArchetype.None, influence);

        Assert.NotNull(hint);
        Assert.Contains("caretaker", hint, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetProjectContributionAgencyFeedback_ReturnsBuilderLineWhenQualified()
    {
        var influence = new LegacyArchetypeInfluence(5, 0, 0);
        var line = LegacyArchetypeAgencyConfig.TryGetProjectContributionAgencyFeedback(
            LegacyArchetype.Builder,
            influence,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains("builder", line, StringComparison.OrdinalIgnoreCase);
    }
}