namespace Bloomtown.Server.Simulation.Gathering;

/// <summary>
/// Simulation tick hook that advances active gathering sessions and node cooldowns.
/// </summary>
public sealed class GatheringSystem : ISimulationSystem
{
    private readonly ResourceGatheringHandler _handler;
    private readonly double _gameMinutesPerRealSecond;

    public GatheringSystem(ResourceGatheringHandler handler, double gameMinutesPerRealSecond = 1.0)
    {
        _handler = handler;
        _gameMinutesPerRealSecond = gameMinutesPerRealSecond;
    }

    public void Update(double deltaTimeSeconds)
    {
        var deltaGameMinutes = deltaTimeSeconds * _gameMinutesPerRealSecond;
        _handler.Update(deltaTimeSeconds, deltaGameMinutes);
    }
}