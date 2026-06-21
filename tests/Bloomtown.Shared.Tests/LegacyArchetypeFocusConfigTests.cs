using Bloomtown.Shared.Goals;

namespace Bloomtown.Shared.Tests;

public sealed class LegacyArchetypeFocusConfigTests
{
    [Fact]
    public void ApplyConsciousInfluenceGain_AddsTwoToFocusedPathAndDriftsOthers()
    {
        var current = new LegacyArchetypeInfluence(5, 4, 3);
        var updated = LegacyArchetypeFocusConfig.ApplyConsciousInfluenceGain(current, LegacyArchetype.Builder);

        Assert.Equal(7, updated.BuilderPoints);
        Assert.Equal(3, updated.CaretakerPoints);
        Assert.Equal(2, updated.ConnectorPoints);
    }

    [Fact]
    public void ApplyConsciousInfluenceGain_DriftDoesNotGoBelowZero()
    {
        var current = new LegacyArchetypeInfluence(0, 0, 1);
        var updated = LegacyArchetypeFocusConfig.ApplyConsciousInfluenceGain(current, LegacyArchetype.Caretaker);

        Assert.Equal(0, updated.BuilderPoints);
        Assert.Equal(2, updated.CaretakerPoints);
        Assert.Equal(0, updated.ConnectorPoints);
    }

    [Fact]
    public void MeetsLocationRequirement_BuilderNearWell()
    {
        Assert.True(LegacyArchetypeFocusConfig.MeetsLocationRequirement(
            LegacyArchetype.Builder,
            playerX: 5f,
            playerZ: 5f,
            hasNearbyNpc: false));
    }

    [Fact]
    public void MeetsLocationRequirement_CaretakerNearGarden()
    {
        Assert.True(LegacyArchetypeFocusConfig.MeetsLocationRequirement(
            LegacyArchetype.Caretaker,
            playerX: 20f,
            playerZ: 14f,
            hasNearbyNpc: false));
    }

    [Fact]
    public void MeetsLocationRequirement_ConnectorAcceptsNearbyNpc()
    {
        Assert.True(LegacyArchetypeFocusConfig.MeetsLocationRequirement(
            LegacyArchetype.Connector,
            playerX: 0f,
            playerZ: 0f,
            hasNearbyNpc: true));
    }

    [Fact]
    public void QualifiesForIdentityRecognition_RequiresDetectedArchetypeAndInfluence()
    {
        var influence = new LegacyArchetypeInfluence(6, 0, 0);

        Assert.True(LegacyArchetypeFocusConfig.QualifiesForIdentityRecognition(
            LegacyArchetype.Builder,
            influence));
        Assert.False(LegacyArchetypeFocusConfig.QualifiesForIdentityRecognition(
            LegacyArchetype.None,
            influence));
        Assert.False(LegacyArchetypeFocusConfig.QualifiesForIdentityRecognition(
            LegacyArchetype.Builder,
            new LegacyArchetypeInfluence(3, 0, 0)));
    }

    [Fact]
    public void FormatActiveFocusStatusLine_ShowsTradeoffHint()
    {
        var influence = new LegacyArchetypeInfluence(8, 2, 1);
        var line = LegacyArchetypeFocusConfig.FormatActiveFocusStatusLine(
            LegacyArchetype.Builder,
            influence);

        Assert.NotNull(line);
        Assert.Contains("Builder", line, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("more slowly", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void TryGetConsciousActionFeedback_ReturnsPathSpecificLine()
    {
        var line = LegacyArchetypeFocusConfig.TryGetConsciousActionFeedback(
            LegacyArchetype.Caretaker,
            variationSeed: 0);

        Assert.False(string.IsNullOrWhiteSpace(line));
        Assert.Contains("caretaker", line, StringComparison.OrdinalIgnoreCase);
    }
}