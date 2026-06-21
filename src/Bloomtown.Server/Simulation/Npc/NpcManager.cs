using Bloomtown.Server.Persistence.Models;
using Bloomtown.Server.Simulation.Aoi;
using Bloomtown.Server.Simulation.Npc.Ai;
using Bloomtown.Server.Simulation.Npc.Movement;
using Bloomtown.Server.Simulation.Npc.Needs;
using Bloomtown.Server.Simulation.Npc.Schedule;
using Bloomtown.Server.Simulation.Npc.Utility;
using Bloomtown.Shared.Protocol;
using Serilog;

namespace Bloomtown.Server.Simulation.Npc;

/// <summary>
/// Spawns NPC simulation bundles and applies persisted state.
/// </summary>
public sealed class NpcManager
{
    private readonly List<NpcSimulationState> _npcs = new();

    public IReadOnlyList<StaticNpc> Npcs => _npcs.Select(state => state.Npc).ToList();

    public IReadOnlyList<NpcSimulationState> SimulationStates => _npcs;

    public NpcSimulationState? GetState(uint entityId)
    {
        return _npcs.FirstOrDefault(state => state.Npc.EntityId == entityId);
    }

    public void SpawnDefaults(AoiSystem aoiSystem)
    {
        Spawn(
            aoiSystem,
            NpcEntityIds.Elsie,
            "Elsie",
            worldX: 8f,
            worldY: 0f,
            worldZ: 8f,
            schedule: NpcDailySchedule.CreateElsieSchedule(),
            personality: NpcPersonalityTrait.Diligent,
            moveSpeed: 2f);

        Spawn(
            aoiSystem,
            NpcEntityIds.Tom,
            "Tom",
            worldX: 100f,
            worldY: 0f,
            worldZ: 100f,
            schedule: NpcDailySchedule.CreateTomSchedule(),
            personality: NpcPersonalityTrait.Social,
            moveSpeed: 1.5f);

        Spawn(
            aoiSystem,
            NpcEntityIds.Mira,
            "Mira",
            worldX: 18f,
            worldY: 0f,
            worldZ: 6f,
            schedule: NpcDailySchedule.CreateMiraSchedule(),
            personality: NpcPersonalityTrait.Social,
            moveSpeed: 1.6f);

        Spawn(
            aoiSystem,
            NpcEntityIds.Harold,
            "Harold",
            worldX: 14f,
            worldY: 0f,
            worldZ: 10f,
            schedule: NpcDailySchedule.CreateHaroldSchedule(),
            personality: NpcPersonalityTrait.Diligent,
            moveSpeed: 1.2f);

        Spawn(
            aoiSystem,
            NpcEntityIds.Greta,
            "Greta",
            worldX: 22f,
            worldY: 0f,
            worldZ: 16f,
            schedule: NpcDailySchedule.CreateGretaSchedule(),
            personality: NpcPersonalityTrait.Social,
            moveSpeed: 1.4f);

        Spawn(
            aoiSystem,
            NpcEntityIds.Nora,
            "Nora",
            worldX: 16f,
            worldY: 0f,
            worldZ: 18f,
            schedule: NpcDailySchedule.CreateNoraSchedule(),
            personality: NpcPersonalityTrait.Diligent,
            moveSpeed: 1.3f);

        Spawn(
            aoiSystem,
            NpcEntityIds.Elias,
            "Elias",
            worldX: 12f,
            worldY: 0f,
            worldZ: 14f,
            schedule: NpcDailySchedule.CreateEliasSchedule(),
            personality: NpcPersonalityTrait.Diligent,
            moveSpeed: 1.3f);

        Spawn(
            aoiSystem,
            NpcEntityIds.Marcus,
            "Marcus",
            worldX: 10f,
            worldY: 0f,
            worldZ: 16f,
            schedule: NpcDailySchedule.CreateMarcusSchedule(),
            personality: NpcPersonalityTrait.Diligent,
            moveSpeed: 1.3f);

        Spawn(
            aoiSystem,
            NpcEntityIds.Ben,
            "Ben",
            worldX: 15f,
            worldY: 0f,
            worldZ: 11f,
            schedule: NpcDailySchedule.CreateBenSchedule(),
            personality: NpcPersonalityTrait.Diligent,
            moveSpeed: 1.4f);

        Spawn(
            aoiSystem,
            NpcEntityIds.Lila,
            "Lila",
            worldX: 19f,
            worldY: 0f,
            worldZ: 10f,
            schedule: NpcDailySchedule.CreateLilaSchedule(),
            personality: NpcPersonalityTrait.Social,
            moveSpeed: 1.35f);

        Spawn(
            aoiSystem,
            NpcEntityIds.Rowan,
            "Rowan",
            worldX: 11f,
            worldY: 0f,
            worldZ: 9f,
            schedule: NpcDailySchedule.CreateRowanSchedule(),
            personality: NpcPersonalityTrait.Social,
            moveSpeed: 1.2f);

        Spawn(
            aoiSystem,
            NpcEntityIds.Eleanor,
            "Eleanor",
            worldX: 13f,
            worldY: 0f,
            worldZ: 12f,
            schedule: NpcDailySchedule.CreateEleanorSchedule(),
            personality: NpcPersonalityTrait.Social,
            moveSpeed: 1.1f);
    }

    public void ApplyPersistedState(IReadOnlyList<NpcRecord> records, AoiSystem aoiSystem)
    {
        foreach (var record in records)
        {
            var state = GetState(record.EntityId);
            if (state is null)
                continue;

            state.Npc.PositionX = record.PositionX;
            state.Npc.PositionY = record.PositionY;
            state.Npc.PositionZ = record.PositionZ;
            state.Needs.Load(record.Hunger, record.Energy, record.Social);
            aoiSystem.SetEntityPosition(record.EntityId, record.PositionX, record.PositionZ);

            Log.Information(
                "Applied persisted state for NPC {NpcName} (entity {EntityId}) pos=({X:F1},{Y:F1},{Z:F1}) hunger={Hunger:F0} energy={Energy:F0} social={Social:F0}",
                state.Npc.Name,
                state.Npc.EntityId,
                state.Npc.PositionX,
                state.Npc.PositionY,
                state.Npc.PositionZ,
                state.Needs.Hunger,
                state.Needs.Energy,
                state.Needs.Social);
        }
    }

    private void Spawn(
        AoiSystem aoiSystem,
        uint entityId,
        string name,
        float worldX,
        float worldY,
        float worldZ,
        NpcDailySchedule schedule,
        NpcPersonalityTrait personality,
        float moveSpeed)
    {
        var npc = new StaticNpc
        {
            EntityId = entityId,
            Name = name,
            PositionX = worldX,
            PositionY = worldY,
            PositionZ = worldZ,
            RotationYaw = 0f,
        };

        var movement = new NpcMovementController(moveSpeed);
        var state = new NpcSimulationState
        {
            Npc = npc,
            Needs = new NpcNeeds(),
            Schedule = schedule,
            Movement = movement,
            Personality = personality,
            CurrentActivity = NpcActivityType.Rest,
        };

        _npcs.Add(state);
        aoiSystem.RegisterEntity(entityId, EntityKind.Npc, worldX, worldZ);

        Log.Information(
            "Spawned NPC {NpcName} (entity {EntityId}) at world ({X:F1}, {Y:F1}, {Z:F1})",
            name,
            entityId,
            worldX,
            worldY,
            worldZ);
    }
}