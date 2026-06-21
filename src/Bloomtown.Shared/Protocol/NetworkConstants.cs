namespace Bloomtown.Shared.Protocol;

public static class NetworkConstants
{
    public const int ServerPort = 7777;
    public const string ConnectionKey = "Bloomtown";

    public const int SimTickRate = 20;
    public const int NetSendRate = 20;

    public const float PlayerMoveSpeed = 5f;

    public const byte ChannelUnreliable = 0;
    public const byte ChannelReliableOrdered = 1;
}