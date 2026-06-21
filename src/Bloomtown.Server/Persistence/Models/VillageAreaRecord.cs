namespace Bloomtown.Server.Persistence.Models;

public sealed class VillageAreaRecord
{
    public byte AreaId { get; init; }
    public DateTime UnlockedAtUtc { get; init; }
}