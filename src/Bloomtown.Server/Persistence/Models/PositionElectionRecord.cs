using Bloomtown.Shared.Leadership;

namespace Bloomtown.Server.Persistence.Models;

public sealed class PositionElectionRecord
{
    public int Id { get; init; }
    public VillagePosition Position { get; init; }
    public uint CandidatePlayerId { get; init; }
    public PositionElectionStatus Status { get; init; }
    public long VotingEndTotalMinutes { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}