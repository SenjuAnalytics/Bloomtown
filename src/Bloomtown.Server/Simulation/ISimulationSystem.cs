namespace Bloomtown.Server.Simulation;

/// <summary>
/// Contract for simulation systems invoked once per fixed sim tick.
/// </summary>
public interface ISimulationSystem
{
    void Update(double deltaTimeSeconds);
}