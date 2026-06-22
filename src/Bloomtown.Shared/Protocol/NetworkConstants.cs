namespace Bloomtown.Shared.Protocol;

public static class NetworkConstants
{
    public const int ServerPort = 7777;
    public const string ConnectionKey = "Bloomtown";

    public const int SimTickRate = 20;
    public const int NetSendRate = 20;

    /// <summary>Kecepatan jalan santai — cocok dengan cadence animasi Walk Mixamo.</summary>
    public const float PlayerMoveSpeed = 2f;

    public const float GroundY = 0f;
    public const float PlayerGravity = -9.81f;
    public const float PlayerJumpSpeed = 5f;

    /// <summary>Pusat klaster NPC desa (world X/Z).</summary>
    public const float VillageCenterX = 14f;
    public const float VillageCenterZ = 12f;

    public const float DefaultSpawnX = VillageCenterX;
    public const float DefaultSpawnY = 0f;
    public const float DefaultSpawnZ = VillageCenterZ;

    public const byte ChannelUnreliable = 0;
    public const byte ChannelReliableOrdered = 1;
}