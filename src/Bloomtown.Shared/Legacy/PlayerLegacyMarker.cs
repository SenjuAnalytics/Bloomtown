namespace Bloomtown.Shared.Legacy;

/// <summary>
/// Lightweight markers recording meaningful player contributions the village remembers.
/// </summary>
[Flags]
public enum PlayerLegacyMarker : ushort
{
    None = 0,

    /// <summary>Player contributed to at least one community project (village-wide memory).</summary>
    HelpedCommunityProject = 1 << 0,

    ContributedToWell = 1 << 1,
    ContributedToBridge = 1 << 2,
    ContributedToWarehouse = 1 << 3,

    HelperTitle = 1 << 4,
    BuilderTitle = 1 << 5,
    RespectedTitle = 1 << 6,
    ElderCandidateTitle = 1 << 7,
}