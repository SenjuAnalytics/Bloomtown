namespace Bloomtown.Shared.Protocol;

public readonly record struct NpcInteractionResponse(
    bool Success,
    NpcInteractionKind Kind,
    uint NpcEntityId,
    NpcInteractionFailureReason FailureReason,
    string Message);