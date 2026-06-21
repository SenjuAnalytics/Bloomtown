using Bloomtown.Shared.Goals;

namespace Bloomtown.Shared.Protocol;

public readonly record struct LegacyFocusRequest(
    LegacyFocusRequestKind Kind,
    LegacyArchetype Path);