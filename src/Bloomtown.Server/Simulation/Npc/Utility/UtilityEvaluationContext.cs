using Bloomtown.Server.Simulation.Npc.Needs;
using Bloomtown.Server.Simulation.Npc.Schedule;

namespace Bloomtown.Server.Simulation.Npc.Utility;

/// <summary>
/// Snapshot of NPC state used while scoring candidate activities.
/// </summary>
public readonly record struct UtilityEvaluationContext(
    uint EntityId,
    float PositionX,
    float PositionZ,
    NpcNeeds Needs,
    NpcDailySchedule Schedule,
    NpcPersonalityTrait Personality,
    int GameMinute);