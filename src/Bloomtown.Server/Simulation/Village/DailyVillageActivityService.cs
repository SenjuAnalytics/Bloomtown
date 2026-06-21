using Bloomtown.Server.Simulation;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Goals;
using Bloomtown.Server.Simulation.Needs;
using Bloomtown.Server.Simulation.Relationship;
using Bloomtown.Server.Simulation.Routines;
using Bloomtown.Shared.Activities;
using Bloomtown.Shared.Console;
using Bloomtown.Shared.Needs;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Village;
using Bloomtown.Shared.World;
using Serilog;

namespace Bloomtown.Server.Simulation.Village;

/// <summary>
/// Validates village leisure spots, applies light need recovery, and records daily rhythm.
/// </summary>
public sealed class DailyVillageActivityService
{
    private readonly PlayerEconomyService _economyService;
    private readonly PlayerNeedsService _needsService;
    private readonly PlayerNpcRelationshipService? _relationshipService;
    private readonly PlayerDailyRhythmTracker _dailyRhythmTracker;
    private readonly WorldTimeSystem _worldTime;
    private readonly PlayerMilestoneService? _milestoneService;
    private readonly Dictionary<(uint PlayerId, DailyVillageActivityKind Kind), DateTime> _cooldowns = new();

    public DailyVillageActivityService(
        PlayerEconomyService economyService,
        PlayerNeedsService needsService,
        PlayerDailyRhythmTracker dailyRhythmTracker,
        WorldTimeSystem worldTime,
        PlayerNpcRelationshipService? relationshipService = null,
        PlayerMilestoneService? milestoneService = null)
    {
        _economyService = economyService;
        _needsService = needsService;
        _dailyRhythmTracker = dailyRhythmTracker;
        _worldTime = worldTime;
        _relationshipService = relationshipService;
        _milestoneService = milestoneService;
    }

    public DailyVillageActivityResponse Handle(
        uint playerEntityId,
        float playerX,
        float playerZ,
        DailyVillageActivityRequest request)
    {
        return request.Kind switch
        {
            DailyVillageActivityRequestKind.List => HandleList(playerEntityId),
            DailyVillageActivityRequestKind.Perform => Perform(playerEntityId, playerX, playerZ, request.Activity),
            _ => Fail(
                DailyVillageActivityRequestKind.List,
                DailyVillageActivityFailureReason.UnknownRequest,
                "Unknown daily village activity request."),
        };
    }

    public string FormatNearbyStatus(float playerX, float playerZ) =>
        DailyVillageActivityConfig.FormatNearbyStatus(playerX, playerZ);

    private DailyVillageActivityResponse HandleList(uint playerEntityId)
    {
        Log.Information("Player {PlayerId} viewed daily village leisure activities.", playerEntityId);
        return new DailyVillageActivityResponse(
            true,
            DailyVillageActivityRequestKind.List,
            DailyVillageActivityFailureReason.None,
            DailyVillageActivityConfig.FormatActivityList());
    }

    private DailyVillageActivityResponse Perform(
        uint playerEntityId,
        float playerX,
        float playerZ,
        DailyVillageActivityKind activity)
    {
        if (!DailyVillageActivityConfig.TryGet(activity, out var definition))
        {
            return Fail(
                DailyVillageActivityRequestKind.Perform,
                DailyVillageActivityFailureReason.UnknownActivity,
                "Unknown daily village activity.");
        }

        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                DailyVillageActivityRequestKind.Perform,
                DailyVillageActivityFailureReason.EconomyUnavailable,
                "Player state is unavailable.");
        }

        if (!DailyVillageActivityConfig.IsAvailableAt(definition, playerX, playerZ))
        {
            var distance = DailyVillageActivityConfig.GetDistance(
                playerX,
                playerZ,
                definition.WorldX,
                definition.WorldZ);
            return Fail(
                DailyVillageActivityRequestKind.Perform,
                DailyVillageActivityFailureReason.NotInRange,
                $"{definition.LocationName} is too far away ({distance:F1}m). Move within {DailyVillageActivityConfig.InteractionRadiusMeters:F0}m of ({definition.WorldX:F0}, {definition.WorldZ:F0}).");
        }

        if (TryGetCooldownFailure(playerEntityId, activity, definition, out var cooldownFailure))
            return cooldownFailure;

        var currentPhase = GameTimeHelper.GetTimeOfDay(_worldTime.GameHour);
        var timedAdjustment = ActivityTimeOfDayConfig.GetVillageAdjustment(activity, currentPhase);
        var standingTier = ResolveSocialStandingTier(playerEntityId);
        var (standingMoodBonus, standingSocialBonus, standingFatigueBonus) =
            VillageSocialStandingMechanicalConfig.GetDailyVillageActivityStandingBonus(activity, standingTier);

        var rhythmBonus = _dailyRhythmTracker.GetActivityBonus(
            playerEntityId,
            definition.RhythmCategory,
            currentPhase,
            _worldTime.GameDay);
        var effectiveMoodGain = definition.MoodGain + timedAdjustment.MoodBonus + standingMoodBonus
            + rhythmBonus.MoodBonus;
        var effectiveFatigueReduction = definition.FatigueReduction + timedAdjustment.FatigueReductionBonus
            + standingFatigueBonus + rhythmBonus.FatigueReductionBonus;
        var effectiveSocialReduction = MathF.Max(
            0f,
            definition.SocialReduction + standingSocialBonus - timedAdjustment.SocialReductionPenalty
            + rhythmBonus.SocialReductionBonus);

        var moodBefore = economy.Mood;
        var fatigueBefore = economy.Fatigue;
        var socialBefore = economy.SocialNeed;

        _needsService.ApplyDailyVillageActivity(
            economy,
            effectiveMoodGain,
            effectiveFatigueReduction,
            effectiveSocialReduction);

        SetCooldown(playerEntityId, activity);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();
        _dailyRhythmTracker.Record(
            playerEntityId,
            definition.RhythmCategory,
            definition.CommandHint,
            currentPhase,
            _worldTime.GameDay);

        Log.Information(
            "Player {PlayerId} daily village activity {Activity} at {Location} [{Phase}] — mood {MoodBefore:F0}->{MoodAfter:F0}, fatigue {FatigueBefore:F0}->{FatigueAfter:F0}, social {SocialBefore:F0}->{SocialAfter:F0}.",
            playerEntityId,
            definition.CommandHint,
            definition.LocationName,
            GameTimeHelper.GetDisplayName(currentPhase),
            moodBefore,
            economy.Mood,
            fatigueBefore,
            economy.Fatigue,
            socialBefore,
            economy.SocialNeed);

        var useStandingFlavor = standingTier >= VillageSocialStandingTier.Respected;
        var flavorSeed = playerEntityId + (uint)activity + (uint)currentPhase + (uint)_cooldowns.Count;
        var flavor = ActivityTimeOfDayConfig.PickVillageTimedFlavor(activity, currentPhase, flavorSeed)
            ?? DailyVillageActivityConfig.PickFlavorText(definition, useStandingFlavor, flavorSeed);
        var standingFeedback = VillageSocialStandingMechanicalConfig.FormatDailyVillageActivityStandingFeedback(
            activity,
            standingTier);
        var timingNote = ActivityTimeOfDayConfig.FormatTimingNote(activity, currentPhase);

        var needLine = ActivityFeedbackFormat.FormatNeedChanges(
            moodBefore,
            economy.Mood,
            fatigueBefore,
            economy.Fatigue,
            socialBefore,
            economy.SocialNeed,
            includeSocial: true);
        var message =
            $"{flavor}{standingFeedback}{timingNote}{rhythmBonus.FeedbackNote}{Environment.NewLine}{needLine}";

        _milestoneService?.RecordDailyVillageActivity(playerEntityId, _worldTime.GameDay);
        _milestoneService?.ReconcileAsync(playerEntityId).GetAwaiter().GetResult();
        if (_milestoneService?.TryConsumePendingFeedback(playerEntityId, out var milestoneFeedback) == true
            && !string.IsNullOrWhiteSpace(milestoneFeedback))
        {
            message = $"{message}{Environment.NewLine}{milestoneFeedback}";
        }

        return new DailyVillageActivityResponse(
            true,
            DailyVillageActivityRequestKind.Perform,
            DailyVillageActivityFailureReason.None,
            message);
    }

    private bool TryGetCooldownFailure(
        uint playerEntityId,
        DailyVillageActivityKind activity,
        DailyVillageActivityDefinition definition,
        out DailyVillageActivityResponse failure)
    {
        var key = (playerEntityId, activity);
        if (_cooldowns.TryGetValue(key, out var lastUsed) && DateTime.UtcNow - lastUsed < definition.Cooldown)
        {
            var remaining = definition.Cooldown - (DateTime.UtcNow - lastUsed);
            failure = Fail(
                DailyVillageActivityRequestKind.Perform,
                DailyVillageActivityFailureReason.OnCooldown,
                $"Please wait {remaining.TotalSeconds:F0}s before '{definition.CommandHint}' again.");
            return true;
        }

        failure = default;
        return false;
    }

    private void SetCooldown(uint playerEntityId, DailyVillageActivityKind activity) =>
        _cooldowns[(playerEntityId, activity)] = DateTime.UtcNow;

    private VillageSocialStandingTier ResolveSocialStandingTier(uint playerEntityId)
    {
        if (_relationshipService is null)
            return VillageSocialStandingTier.Stranger;

        return VillageSocialStandingConfig.ResolveTier(
            npcId => _relationshipService.GetTier(playerEntityId, npcId));
    }

    private static DailyVillageActivityResponse Fail(
        DailyVillageActivityRequestKind kind,
        DailyVillageActivityFailureReason reason,
        string message)
    {
        Log.Information("Daily village activity request {Kind} failed ({Reason}): {Message}", kind, reason, message);
        return new DailyVillageActivityResponse(false, kind, reason, message);
    }
}