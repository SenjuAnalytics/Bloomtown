namespace Bloomtown.Shared.Protocol;

public enum ClientQueryKind : byte
{
    Status = 1,
    Nearby = 2,
    Nodes = 3,
    Rest = 4,
    Sleep = 5,
    Goal = 6,
}