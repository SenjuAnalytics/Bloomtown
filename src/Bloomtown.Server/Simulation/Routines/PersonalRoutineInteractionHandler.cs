using Bloomtown.Shared.Protocol;

namespace Bloomtown.Server.Simulation.Routines;

/// <summary>
/// Handles client requests to list and perform personal routines.
/// </summary>
public sealed class PersonalRoutineInteractionHandler
{
    private readonly PlayerRoutineService _routineService;

    public PersonalRoutineInteractionHandler(PlayerRoutineService routineService)
    {
        _routineService = routineService;
    }

    public PersonalRoutineResponse Handle(uint playerEntityId, PersonalRoutineRequest request) =>
        _routineService.Handle(playerEntityId, request);
}