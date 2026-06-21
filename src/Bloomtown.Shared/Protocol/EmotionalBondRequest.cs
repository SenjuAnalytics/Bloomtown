namespace Bloomtown.Shared.Protocol;

public readonly record struct EmotionalBondRequest(
    EmotionalBondRequestKind Kind,
    EmotionalBondActionKind Action,
    uint TargetNpcEntityId);