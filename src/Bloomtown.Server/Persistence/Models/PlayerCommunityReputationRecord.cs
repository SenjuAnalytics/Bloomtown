namespace Bloomtown.Server.Persistence.Models;

public sealed class PlayerCommunityReputationRecord
{
    public required uint PlayerEntityId { get; init; }
    public int HelpGardenCount { get; init; }
    public int HelpMarketCount { get; init; }
    public int HelpWellCount { get; init; }
}