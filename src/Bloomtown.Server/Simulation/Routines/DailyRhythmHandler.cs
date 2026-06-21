using Bloomtown.Shared.Protocol;

namespace Bloomtown.Server.Simulation.Routines;

public sealed class DailyRhythmHandler
{
    private readonly DailyRhythmService _rhythmService;

    public DailyRhythmHandler(DailyRhythmService rhythmService)
    {
        _rhythmService = rhythmService;
    }

    public DailyRhythmResponse Handle(uint playerEntityId, DailyRhythmRequest request) =>
        _rhythmService.Handle(playerEntityId, request);
}