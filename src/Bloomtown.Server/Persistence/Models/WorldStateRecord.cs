using Bloomtown.Shared.Community;

namespace Bloomtown.Server.Persistence.Models;

public sealed class WorldStateRecord
{
    public int Id { get; init; } = 1;
    public int GameDay { get; init; }
    public int GameMinute { get; init; }
    public DateTime LastSavedUtc { get; init; }
    public VillageDevelopmentLevel VillageDevelopmentLevel { get; init; } = VillageDevelopmentLevel.Quiet;
}