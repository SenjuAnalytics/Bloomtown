using Bloomtown.Shared.Milestone;

namespace Bloomtown.Shared.Protocol;

public readonly record struct MilestoneRequest(
    MilestoneRequestKind Kind,
    MilestoneInteractionKind Interaction);