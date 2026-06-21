using Bloomtown.Server.Simulation;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Needs;
using Bloomtown.Server.Simulation.Relationship;
using Bloomtown.Server.Simulation.Routines;
using Bloomtown.Shared.Activities;
using Bloomtown.Shared.Console;
using Bloomtown.Shared.Housing;
using Bloomtown.Shared.Needs;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;
using Bloomtown.Shared.World;
using Serilog;

namespace Bloomtown.Server.Simulation.Housing;

/// <summary>
/// Validates at-home location and furniture bonuses, then applies cozy activity recovery.
/// </summary>
public sealed class HomeActivityService
{
    private readonly PlayerHousingService _housingService;
    private readonly PlayerEconomyService _economyService;
    private readonly PlayerNeedsService _needsService;
    private readonly PlayerDailyRhythmTracker _dailyRhythmTracker;
    private readonly WorldTimeSystem _worldTime;
    private readonly PlayerNpcRelationshipService? _relationshipService;

    public HomeActivityService(
        PlayerHousingService housingService,
        PlayerEconomyService economyService,
        PlayerNeedsService needsService,
        PlayerDailyRhythmTracker dailyRhythmTracker,
        WorldTimeSystem worldTime,
        PlayerNpcRelationshipService? relationshipService = null)
    {
        _housingService = housingService;
        _economyService = economyService;
        _needsService = needsService;
        _dailyRhythmTracker = dailyRhythmTracker;
        _worldTime = worldTime;
        _relationshipService = relationshipService;
    }

    public HomeResponse TryPerformActivity(
        uint playerEntityId,
        float playerX,
        float playerZ,
        HomeActivityType activityType)
    {
        if (!HomeActivityConfig.IsKnownActivity(activityType))
        {
            return Fail(HomeFailureReason.UnknownActivity, "Unknown home activity.");
        }

        if (!_housingService.TryGetState(playerEntityId, out var house))
        {
            return Fail(HomeFailureReason.HomeUnavailable, "Home data is unavailable.");
        }

        if (!PlayerHousingConfig.IsWithinHome(playerX, playerZ, house.HouseX, house.HouseZ))
        {
            var distance = PlayerHousingConfig.GetDistance(playerX, playerZ, house.HouseX, house.HouseZ);
            return Fail(
                HomeFailureReason.NotAtHome,
                $"You must be at your home ({house.HouseX:F0}, {house.HouseZ:F0}) for cozy activities ({distance:F1}m away). " +
                $"Move within {PlayerHousingConfig.AccessRadiusMeters:F0}m.");
        }

        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(HomeFailureReason.HomeUnavailable, "Player state is unavailable.");
        }

        var currentPhase = GameTimeHelper.GetTimeOfDay(_worldTime.GameHour);
        var effects = HomeActivityConfig.CalculateEffects(activityType, house.PlacedFurniture);
        if (effects.MoodGain <= 0f && effects.FatigueReduction <= 0f)
        {
            return Fail(HomeFailureReason.UnknownActivity, "This activity has no recovery effect.");
        }

        var timedAdjustment = ActivityTimeOfDayConfig.GetHomeAdjustment(activityType, currentPhase);
        var standingTier = ResolveSocialStandingTier(playerEntityId);
        var standingMoodBonus = activityType == HomeActivityType.Nap
            ? VillageSocialStandingMechanicalConfig.GetHomeNapStandingMoodBonus(standingTier)
            : 0f;
        var rhythmBonus = _dailyRhythmTracker.GetActivityBonus(
            playerEntityId,
            DailyRhythmActivityCategory.Recovery,
            currentPhase,
            _worldTime.GameDay);
        var effectiveMoodGain = effects.MoodGain + timedAdjustment.MoodBonus + standingMoodBonus
            + rhythmBonus.MoodBonus;
        var effectiveFatigueReduction = effects.FatigueReduction + timedAdjustment.FatigueReductionBonus
            + rhythmBonus.FatigueReductionBonus;

        var moodBefore = economy.Mood;
        var fatigueBefore = economy.Fatigue;
        _needsService.ApplyHomeActivity(economy, effectiveMoodGain, effectiveFatigueReduction);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        var commandName = HomeActivityConfig.GetCommandName(activityType);
        _dailyRhythmTracker.Record(
            playerEntityId,
            DailyRhythmActivityCategory.Recovery,
            commandName,
            currentPhase,
            _worldTime.GameDay);

        var moodDelta = economy.Mood - moodBefore;
        var fatigueDelta = fatigueBefore - economy.Fatigue;
        var flavorSeed = playerEntityId + (uint)activityType + (uint)currentPhase + (uint)_worldTime.GameDay;
        var flavor = ActivityTimeOfDayConfig.PickHomeTimedFlavor(activityType, currentPhase, flavorSeed)
            ?? HomeActivityConfig.PickFlavorText(activityType, effects.BonusFurnitureApplied, flavorSeed);
        var bonusNote = effects.BonusFurnitureApplied && effects.BonusFurniture is FurnitureType furniture
            ? $" ({FurnitureTypeDisplay.GetName(furniture)} bonus!)"
            : string.Empty;
        var standingFeedback = activityType == HomeActivityType.Nap
            ? VillageSocialStandingMechanicalConfig.FormatHomeNapStandingFeedback(standingTier)
            : string.Empty;
        var timingNote = ActivityTimeOfDayConfig.FormatTimingNote(activityType, currentPhase);

        var needLine = ActivityFeedbackFormat.FormatNeedChanges(
            moodBefore,
            economy.Mood,
            fatigueBefore,
            economy.Fatigue);
        var message =
            $"{flavor}{bonusNote}{standingFeedback}{timingNote}{rhythmBonus.FeedbackNote}{Environment.NewLine}{needLine}";

        Log.Information(
            "Player {PlayerId} home activity {Activity} at ({HouseX:F0}, {HouseZ:F0}) [{Phase}] — Mood {MoodBefore:F0}->{MoodAfter:F0} (+{MoodGain:F0}), Fatigue {FatigueBefore:F0}->{FatigueAfter:F0} (-{FatigueDrop:F0}){Bonus}.",
            playerEntityId,
            activityType,
            house.HouseX,
            house.HouseZ,
            GameTimeHelper.GetDisplayName(currentPhase),
            moodBefore,
            economy.Mood,
            moodDelta,
            fatigueBefore,
            economy.Fatigue,
            fatigueDelta,
            effects.BonusFurnitureApplied ? $" [{effects.BonusFurniture} bonus]" : string.Empty);

        return new HomeResponse(true, HomeRequestKind.Activity, HomeFailureReason.None, message);
    }

    private VillageSocialStandingTier ResolveSocialStandingTier(uint playerEntityId)
    {
        if (_relationshipService is null)
            return VillageSocialStandingTier.Stranger;

        return VillageSocialStandingConfig.ResolveTier(
            npcId => _relationshipService.GetTier(playerEntityId, npcId));
    }

    private static HomeResponse Fail(HomeFailureReason reason, string message)
    {
        Log.Information("Home activity failed ({Reason}): {Message}", reason, message);
        return new HomeResponse(false, HomeRequestKind.Activity, reason, message);
    }
}