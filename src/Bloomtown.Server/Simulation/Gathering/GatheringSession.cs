namespace Bloomtown.Server.Simulation.Gathering;

/// <summary>
/// Tracks an in-progress gather action for one player.
/// </summary>
public sealed class GatheringSession
{
    public required uint PlayerEntityId { get; init; }
    public required int NodeId { get; init; }
    public required double RemainingRealSeconds { get; set; }
}