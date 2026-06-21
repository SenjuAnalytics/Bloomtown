namespace Bloomtown.Shared.Protocol;

public readonly record struct NpcInteractionRequest(
    NpcInteractionKind Kind,
    uint TargetNpcEntityId = 0);