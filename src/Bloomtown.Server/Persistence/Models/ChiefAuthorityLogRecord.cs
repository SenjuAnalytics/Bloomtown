using Bloomtown.Shared.Community;

namespace Bloomtown.Server.Persistence.Models;

public sealed class ChiefAuthorityLogRecord
{
    public int Id { get; init; }
    public uint ChiefPlayerId { get; init; }
    public ChiefAuthorityAction ActionType { get; init; }
    public int ProposalId { get; init; }
    public int GameDay { get; init; }
    public DateTime CreatedAtUtc { get; init; }
}