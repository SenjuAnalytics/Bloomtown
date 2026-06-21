using Bloomtown.Server.Simulation;
using Bloomtown.Server.Simulation.Housing;
using Bloomtown.Server.Simulation.Needs;
using Bloomtown.Shared.Needs;
using Bloomtown.Shared.Protocol;
using Serilog;

namespace Bloomtown.Server.Simulation.Economy;

/// <summary>
/// Advances player energy and hunger over game time; handles outdoor rest recovery.
/// </summary>
public sealed class PlayerEnergySystem : ISimulationSystem
{
    private readonly PlayerEconomyService _economyService;
    private readonly PlayerNeedsService _needsService;
    private readonly double _gameMinutesPerRealSecond;
    private readonly HashSet<uint> _lowEnergyWarned = new();
    private readonly HashSet<uint> _highHungerWarned = new();

    public PlayerEnergySystem(
        PlayerEconomyService economyService,
        PlayerNeedsService needsService,
        double gameMinutesPerRealSecond = 1.0)
    {
        _economyService = economyService;
        _needsService = needsService;
        _gameMinutesPerRealSecond = gameMinutesPerRealSecond;
    }

    public void Update(double deltaTimeSeconds)
    {
        if (deltaTimeSeconds <= 0)
            return;

        var deltaGameMinutes = deltaTimeSeconds * _gameMinutesPerRealSecond;

        foreach (var playerEntityId in _economyService.GetCachedPlayerIds())
        {
            if (!_economyService.TryGetState(playerEntityId, out var economy))
                continue;

            var energyDecayRate = PlayerEnergyConfig.GetDecayPerGameMinute(economy.Energy);
            var energyBefore = economy.Energy;
            economy.Energy = PlayerEnergyConfig.Clamp(
                economy.Energy - energyDecayRate * (float)deltaGameMinutes);

            if (energyBefore > PlayerEnergyConfig.LowEnergyThreshold
                && economy.Energy <= PlayerEnergyConfig.LowEnergyThreshold
                && _lowEnergyWarned.Add(playerEntityId))
            {
                Log.Information(
                    "Player {PlayerId} energy dropped to low level ({Energy:F0}/{Max:F0}) — gathering penalty active.",
                    playerEntityId,
                    economy.Energy,
                    PlayerEnergyConfig.MaxValue);
            }

            if (economy.Energy > PlayerEnergyConfig.LowEnergyThreshold)
                _lowEnergyWarned.Remove(playerEntityId);

            // Hunger rises passively over game time (higher = hungrier).
            var hungerBefore = economy.Hunger;
            economy.Hunger = PlayerHungerConfig.Clamp(
                economy.Hunger + PlayerHungerConfig.HungerRisePerGameMinute * (float)deltaGameMinutes);

            if (hungerBefore < PlayerHungerConfig.HighHungerThreshold
                && economy.Hunger >= PlayerHungerConfig.HighHungerThreshold
                && _highHungerWarned.Add(playerEntityId))
            {
                Log.Information(
                    "Player {PlayerId} hunger reached high level ({Hunger:F0}/{Max:F0}).",
                    playerEntityId,
                    economy.Hunger,
                    PlayerHungerConfig.MaxValue);
            }

            if (economy.Hunger < PlayerHungerConfig.HighHungerThreshold)
                _highHungerWarned.Remove(playerEntityId);
        }
    }

    public void ClearPlayer(uint playerEntityId)
    {
        _lowEnergyWarned.Remove(playerEntityId);
        _highHungerWarned.Remove(playerEntityId);
        _needsService.ClearPlayer(playerEntityId);
    }

    /// <summary>
    /// Restores energy via the rest/sleep console command (location-unrestricted for this spike).
    /// </summary>
    public ClientQueryResponse Rest(uint playerEntityId)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return new ClientQueryResponse(
                ClientQueryKind.Rest,
                false,
                "Player state is unavailable.");
        }

        if (economy.Energy >= PlayerEnergyConfig.MaxValue)
        {
            return new ClientQueryResponse(
                ClientQueryKind.Rest,
                true,
                $"You are already fully rested (Energy {economy.Energy:F0}/{PlayerEnergyConfig.MaxValue:F0}).");
        }

        var before = economy.Energy;
        economy.Energy = PlayerEnergyConfig.Clamp(
            economy.Energy + PlayerHousingConfig.RestEnergyRecovery);
        _needsService.ApplyRest(economy);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        var restored = economy.Energy - before;
        _lowEnergyWarned.Remove(playerEntityId);

        Log.Information(
            "Player {PlayerId} rested and recovered +{Restore:F0} energy (now {Energy:F0}/{Max:F0}).",
            playerEntityId,
            restored,
            economy.Energy,
            PlayerEnergyConfig.MaxValue);

        return new ClientQueryResponse(
            ClientQueryKind.Rest,
            true,
            $"You rest outdoors for about {PlayerHousingConfig.RestDurationGameMinutes} game minutes. " +
            $"(Cozy home activities like 'relax' restore more mood and fatigue.) " +
            $"Energy +{restored:F0} (now {economy.Energy:F0}/{PlayerEnergyConfig.MaxValue:F0}). " +
            $"Mood +{PlayerNeedsConfig.RestMoodGain:F0}, Fatigue -{PlayerNeedsConfig.RestFatigueReduction:F0}.");
    }
}