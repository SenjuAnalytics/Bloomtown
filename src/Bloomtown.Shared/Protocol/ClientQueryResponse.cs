namespace Bloomtown.Shared.Protocol;

public readonly record struct ClientQueryResponse(
    ClientQueryKind Kind,
    bool Success,
    string Message);