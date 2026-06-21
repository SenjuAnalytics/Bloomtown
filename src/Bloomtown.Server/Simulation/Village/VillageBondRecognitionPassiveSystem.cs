using Bloomtown.Server.Simulation.Housing;
using Bloomtown.Server.Simulation.Memory;
using Bloomtown.Server.Simulation.Needs;
using Bloomtown.Server.Simulation.Relationship;
using Bloomtown.Shared.Relationship;
using Bloomtown.Shared.Village;
using Serilog;

namespace Bloomtown.Server.Simulation.Village;

/// <summary>
/// Applies very light passive mood recovery while players with multiple close focus bonds
/// linger in the village (outside their home).
/// </summary>
public sealed class VillageBondRecognitionPassiveSystem : ISimulationSystem
{
    private readonly PlayerHousingService _housingService;
    private readonly PlayerNpcRelationshipService _relationshipService;
    private readonly NpcMemoryService _memoryService;
    private readonly PlayerNeedsService _needsService;
    private readonly Func<IReadOnlyList<(uint EntityId, float X, float Z)>> _getPlayerPositions;
    private readonly double _gameMinutesPerRealSecond;

    public VillageBondRecognitionPassiveSystem(
        PlayerHousingService housingService,
        PlayerNpcRelationshipService relationshipService,
        NpcMemoryService memoryService,
        PlayerNeedsService needsService,
        Func<IReadOnlyList<(uint EntityId, float X, float Z)>> getPlayerPositions,
        double gameMinutesPerRealSecond = 1.0)
    {
        _housingService = housingService;
        _relationshipService = relationshipService;
        _memoryService = memoryService;
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
            if (IsAtHome(playerEntityId, playerX, playerZ))
                continue;

            var focusCloseFriendCount = VillageBondRecognitionConfig.CountFocusCloseFriends(
                id => _relationshipService.GetTier(playerEntityId, id));
            if (!VillageBondRecognitionConfig.IsEligibleForPassiveBenefit(focusCloseFriendCount))
                continue;

            var villageNoticed = _memoryService.HasVillageNoticedBondsMemory(playerEntityId);
            var standingTier = VillageSocialStandingConfig.ResolveTier(focusCloseFriendCount);
            var moodGain = VillageBondRecognitionConfig.GetPassiveMoodRecoveryPerGameMinute(
                focusCloseFriendCount,
                villageNoticed) * (float)deltaGameMinutes;
            moodGain += SocialLegacyConfig.GetPassiveMoodRecoveryPerGameMinute(standingTier)
                * (float)deltaGameMinutes;

            if (moodGain <= 0f)
                continue;

            _needsService.ApplyVillageBondRecognitionPassiveRelief(playerEntityId, moodGain);

            Log.Debug(
                "Player {PlayerId} village bond passive — mood +{MoodGain:F3} ({CloseFriendCount} close focus bonds, noticed={Noticed}).",
                playerEntityId,
                moodGain,
                focusCloseFriendCount,
                villageNoticed);
        }
    }

    private bool IsAtHome(uint playerEntityId, float playerX, float playerZ)
    {
        if (!_housingService.TryGetState(playerEntityId, out var house))
            return false;

        return PlayerHousingConfig.IsWithinHome(playerX, playerZ, house.HouseX, house.HouseZ);
    }
}