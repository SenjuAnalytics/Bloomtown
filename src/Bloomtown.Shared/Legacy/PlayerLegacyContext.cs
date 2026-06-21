using Bloomtown.Shared.Contribution;

namespace Bloomtown.Shared.Legacy;

/// <summary>
/// Computed legacy markers and village regard for a single player.
/// </summary>
public sealed class PlayerLegacyContext
{
    public PlayerLegacyMarker Markers { get; init; }
    public VillageTitle VillageTitle { get; init; }
    public int VillageContributionScore { get; init; }
    public IReadOnlyList<byte> CompletedProjectContributions { get; init; } = Array.Empty<byte>();

    public bool HasRecognition =>
        VillageTitle > VillageTitle.Newcomer
        || Markers.HasFlag(PlayerLegacyMarker.HelpedCommunityProject)
        || HasCompletedProjectContribution;

    private bool HasCompletedProjectContribution =>
        (Markers & (PlayerLegacyMarker.ContributedToWell
                   | PlayerLegacyMarker.ContributedToBridge
                   | PlayerLegacyMarker.ContributedToWarehouse)) != PlayerLegacyMarker.None;
}