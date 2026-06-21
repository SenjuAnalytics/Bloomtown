using Bloomtown.Server.Simulation.Needs;
using Bloomtown.Shared.Village;
using Serilog;

namespace Bloomtown.Server.Simulation.Village;

/// <summary>
/// Applies gentle passive Mood/Fatigue relief while players linger inside unlocked village areas.
/// </summary>
public sealed class VillageAreaPassiveSystem : ISimulationSystem
{
    private readonly VillageAreaService _areaService;
    private readonly PlayerNeedsService _needsService;
    private readonly Func<IReadOnlyList<(uint EntityId, float X, float Z)>> _getPlayerPositions;
    private readonly double _gameMinutesPerRealSecond;

    public VillageAreaPassiveSystem(
        VillageAreaService areaService,
        PlayerNeedsService needsService,
        Func<IReadOnlyList<(uint EntityId, float X, float Z)>> getPlayerPositions,
        double gameMinutesPerRealSecond = 1.0)
    {
        _areaService = areaService;
        _needsService = needsService;
        _getPlayerPositions = getPlayerPositions;
        _gameMinutesPerRealSecond = gameMinutesPerRealSecond;
    }

    public void Update(double deltaTimeSeconds)
    {
        if (deltaTimeSeconds <= 0)
            return;

        var deltaGameMinutes = deltaTimeSeconds * _gameMinutesPerRealSecond;
        if (deltaGameMinutes <= 0)
            return;

        foreach (var (playerEntityId, playerX, playerZ) in _getPlayerPositions())
        {
            if (!_areaService.TryGetPassiveAreaAtPosition(playerX, playerZ, out var areaDefinition))
                continue;

            // Passive benefit: small Mood/Fatigue relief while lingering in an unlocked area.
            var moodGain = areaDefinition.PassiveMoodRecoveryPerGameMinute * (float)deltaGameMinutes;
            var fatigueRelief = areaDefinition.PassiveFatigueRecoveryPerGameMinute * (float)deltaGameMinutes;
            if (moodGain <= 0f && fatigueRelief <= 0f)
                continue;

            _needsService.ApplyAreaPassiveRelief(playerEntityId, moodGain, fatigueRelief);

            Log.Debug(
                "Player {PlayerId} area passive at {AreaName} — mood +{MoodGain:F3}, fatigue -{FatigueRelief:F3}.",
                playerEntityId,
                areaDefinition.Name,
                moodGain,
                fatigueRelief);
        }
    }
}