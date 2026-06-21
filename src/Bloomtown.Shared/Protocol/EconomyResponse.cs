namespace Bloomtown.Shared.Protocol;

public readonly record struct EconomyResponse(
    bool Success,
    EconomyRequestKind Kind,
    uint NpcEntityId,
    EconomyFailureReason FailureReason,
    string Message);