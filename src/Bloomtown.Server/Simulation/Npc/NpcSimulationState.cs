using Bloomtown.Server.Simulation.Npc.Movement;
using Bloomtown.Server.Simulation.Npc.Needs;
using Bloomtown.Server.Simulation.Npc.Schedule;
using Bloomtown.Server.Simulation.Npc.Utility;

namespace Bloomtown.Server.Simulation.Npc;

/// <summary>
/// Runtime bundle for one NPC: identity, needs, schedule, and movement.
/// </summary>
public sealed class NpcSimulationState
{
    public required StaticNpc Npc { get; init; }
    public required NpcNeeds Needs { get; init; }
    public required NpcDailySchedule Schedule { get; init; }
    public required NpcMovementController Movement { get; init; }
    public required NpcPersonalityTrait Personality { get; init; }
    public NpcActivityType CurrentActivity { get; set; } = NpcActivityType.Rest;
}