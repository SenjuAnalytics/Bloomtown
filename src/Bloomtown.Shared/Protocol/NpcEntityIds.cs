namespace Bloomtown.Shared.Protocol;

/// <summary>
/// Reserved entity id range for static and simulated NPCs.
/// Player ids start at 1 and stay below this offset.
/// </summary>
public static class NpcEntityIds
{
    public const uint IdOffset = 10_000;
    public const uint Elsie = 10_001;
    public const uint Tom = 10_002;
    public const uint Mira = 10_003;
    public const uint Harold = 10_004;
    public const uint Greta = 10_005;
    public const uint Nora = 10_006;
    public const uint Elias = 10_007;
    public const uint Ben = 10_008;
    public const uint Lila = 10_009;
    public const uint Rowan = 10_010;
    public const uint Marcus = 10_011;
    public const uint Eleanor = 10_012;

    public static bool IsNpc(uint entityId) => entityId >= IdOffset;

    /// <summary>NPC perempuan memakai model tubuh yang sama dengan pemain (PlayerModel.fbx).</summary>
    public static bool UsesPlayerBodyModel(uint entityId) =>
        entityId is Elsie or Mira or Greta or Nora or Lila or Eleanor;
}