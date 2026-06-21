using Bloomtown.Shared.Community;
using Bloomtown.Shared.Contribution;
using Bloomtown.Shared.Legacy;
using Bloomtown.Shared.Memory;
using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Tests;

public sealed class PlayerLegacyConfigTests
{
    [Fact]
    public void BuildContext_MarksCompletedProjectContributions()
    {
        var context = PlayerLegacyConfig.BuildContext(
            VillageTitle.Builder,
            villageContributionScore: 160,
            [NpcMemoryType.HelpedVillageProject],
            [VillageProjectBenefitConfig.BridgeProjectId]);

        Assert.True(context.HasRecognition);
        Assert.True(context.Markers.HasFlag(PlayerLegacyMarker.ContributedToBridge));
        Assert.True(context.Markers.HasFlag(PlayerLegacyMarker.HelpedCommunityProject));
        Assert.True(context.Markers.HasFlag(PlayerLegacyMarker.BuilderTitle));
    }

    [Fact]
    public void TrySelectRecognitionMarker_PrefersProjectOverTitle()
    {
        var context = PlayerLegacyConfig.BuildContext(
            VillageTitle.ElderCandidate,
            villageContributionScore: 500,
            [NpcMemoryType.HelpedVillageProject],
            [VillageProjectBenefitConfig.WellProjectId]);

        var marker = PlayerLegacyConfig.TrySelectRecognitionMarker(context);

        Assert.Equal(PlayerLegacyMarker.ContributedToWell, marker);
    }

    [Fact]
    public void TryGetElderRecognitionLine_RequiresElderVoiceNpc()
    {
        var context = PlayerLegacyConfig.BuildContext(
            VillageTitle.Helper,
            villageContributionScore: 55,
            [NpcMemoryType.HelpedVillageProject],
            Array.Empty<byte>());

        Assert.True(PlayerLegacyConfig.IsElderVoiceNpc(NpcEntityIds.Elsie));
        Assert.False(PlayerLegacyConfig.IsElderVoiceNpc(NpcEntityIds.Tom));

        var line = PlayerLegacyConfig.TryGetElderRecognitionLine(
            NpcInteractionKind.Talk,
            context,
            variationSeed: 0);

        Assert.NotNull(line);
        Assert.Contains("village", line, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatCommunityRecognitionStatus_ShowsHelperRegard()
    {
        var context = PlayerLegacyConfig.BuildContext(
            VillageTitle.Helper,
            villageContributionScore: 50,
            [NpcMemoryType.HelpedVillageProject],
            Array.Empty<byte>());

        var status = PlayerLegacyConfig.FormatCommunityRecognitionStatus(context);

        Assert.NotNull(status);
        Assert.Contains("appreciate your help", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void FormatCommunityRecognitionStatus_ShowsBuilderRegardForProjectContributors()
    {
        var context = PlayerLegacyConfig.BuildContext(
            VillageTitle.Newcomer,
            villageContributionScore: 0,
            Array.Empty<NpcMemoryType>(),
            [VillageProjectBenefitConfig.WarehouseProjectId]);

        var status = PlayerLegacyConfig.FormatCommunityRecognitionStatus(context);

        Assert.NotNull(status);
        Assert.Contains("projects you helped complete", status, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void ShouldTriggerInteractionRecognition_UsesLowThreshold()
    {
        var roll = (5u * 41 + NpcEntityIds.Elsie * 23 + (uint)(100 % 997)) % 100;
        Assert.Equal(roll < PlayerLegacyConfig.InteractionRecognitionChancePercent,
            PlayerLegacyConfig.ShouldTriggerInteractionRecognition(5, NpcEntityIds.Elsie, 100));
    }

    [Fact]
    public void ShouldTriggerAmbientRecognition_UsesLowThreshold()
    {
        var roll = (5u * 53 + (uint)(100 % 983)) % 100;
        Assert.Equal(roll < PlayerLegacyConfig.AmbientRecognitionChancePercent,
            PlayerLegacyConfig.ShouldTriggerAmbientRecognition(5, 100));
    }

    [Fact]
    public void BuildContext_NewcomerWithoutContributions_HasNoRecognition()
    {
        var context = PlayerLegacyConfig.BuildContext(
            VillageTitle.Newcomer,
            villageContributionScore: 0,
            Array.Empty<NpcMemoryType>(),
            Array.Empty<byte>());

        Assert.False(context.HasRecognition);
        Assert.Null(PlayerLegacyConfig.FormatCommunityRecognitionStatus(context));
    }
}