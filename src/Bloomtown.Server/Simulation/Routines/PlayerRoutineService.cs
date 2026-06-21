using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Server.Simulation.Needs;
using Bloomtown.Server.Simulation.World;
using Bloomtown.Shared.Needs;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Routines;
using Bloomtown.Shared.World;
using Serilog;

namespace Bloomtown.Server.Simulation.Routines;

/// <summary>
/// Validates personal routine timing/cooldowns and applies Mood/Fatigue recovery.
/// </summary>
public sealed class PlayerRoutineService
{
    private readonly PlayerEconomyService _economyService;
    private readonly PlayerNeedsService _needsService;
    private readonly WorldTimeSystem _worldTime;
    private readonly Dictionary<(uint PlayerId, PersonalRoutineKind Kind), DateTime> _cooldowns = new();

    public PlayerRoutineService(
        PlayerEconomyService economyService,
        PlayerNeedsService needsService,
        WorldTimeSystem worldTime)
    {
        _economyService = economyService;
        _needsService = needsService;
        _worldTime = worldTime;
    }

    public GameTimeOfDay CurrentTimeOfDay => _worldTime.CurrentTimeOfDay;

    public PersonalRoutineResponse Handle(uint playerEntityId, PersonalRoutineRequest request)
    {
        return request.Kind switch
        {
            PersonalRoutineRequestKind.List => HandleList(),
            PersonalRoutineRequestKind.Perform => Perform(playerEntityId, request.Routine),
            _ => Fail(
                PersonalRoutineRequestKind.List,
                PersonalRoutineFailureReason.UnknownRequest,
                "Unknown personal routine request."),
        };
    }

    public string FormatPersonalRhythmStatus() =>
        PersonalRoutineConfig.FormatPersonalRhythmStatus(CurrentTimeOfDay);

    private PersonalRoutineResponse HandleList()
    {
        var message = PersonalRoutineConfig.FormatRoutineList(CurrentTimeOfDay);
        return new PersonalRoutineResponse(
            true,
            PersonalRoutineRequestKind.List,
            PersonalRoutineFailureReason.None,
            message);
    }

    private PersonalRoutineResponse Perform(uint playerEntityId, PersonalRoutineKind routine)
    {
        if (!PersonalRoutineConfig.TryGet(routine, out var definition))
        {
            return Fail(
                PersonalRoutineRequestKind.Perform,
                PersonalRoutineFailureReason.UnknownRoutine,
                "Unknown personal routine.");
        }

        if (!_economyService.TryGetState(playerEntityId, out var economy))
        {
            return Fail(
                PersonalRoutineRequestKind.Perform,
                PersonalRoutineFailureReason.EconomyUnavailable,
                "Player state is unavailable.");
        }

        if (TryGetCooldownFailure(playerEntityId, routine, definition, out var cooldownFailure))
            return cooldownFailure;

        // Time-of-day phase adjusts recovery — ideal phases feel noticeably better.
        var currentPhase = CurrentTimeOfDay;
        var effects = PersonalRoutineConfig.CalculateEffects(definition, currentPhase);

        var moodBefore = economy.Mood;
        var fatigueBefore = economy.Fatigue;
        _needsService.ApplyPersonalRoutine(economy, effects.MoodGain, effects.FatigueReduction);
        SetCooldown(playerEntityId, routine);
        _economyService.SavePlayerAsync(playerEntityId).GetAwaiter().GetResult();

        Log.Information(
            "Player {PlayerId} personal routine {Routine} ({Phase}, ideal={Ideal}) — mood {MoodBefore:F0}->{MoodAfter:F0}, fatigue {FatigueBefore:F0}->{FatigueAfter:F0}.",
            playerEntityId,
            definition.CommandHint,
            GameTimeHelper.GetDisplayName(currentPhase),
            effects.IdealPhase,
            moodBefore,
            economy.Mood,
            fatigueBefore,
            economy.Fatigue);

        var flavorSeed = playerEntityId + (uint)routine + (uint)currentPhase + (uint)_worldTime.GameDay;
        var flavor = PersonalRoutineConfig.PickFlavorText(definition, effects.IdealPhase, flavorSeed);
        var timingNote = effects.IdealPhase
            ? $" ({GameTimeHelper.GetDisplayName(currentPhase)} — ideal timing!)"
            : $" ({GameTimeHelper.GetDisplayName(currentPhase)} — softer effect off-peak)";

        var moodDelta = economy.Mood - moodBefore;
        var fatigueDelta = fatigueBefore - economy.Fatigue;
        var message =
            $"{flavor}{timingNote}{Environment.NewLine}" +
            $"Mood +{moodDelta:F0} (now {economy.Mood:F0}/{PlayerNeedsConfig.MaxValue:F0}). " +
            $"Fatigue -{fatigueDelta:F0} (now {economy.Fatigue:F0}/{PlayerNeedsConfig.MaxValue:F0}).";

        return new PersonalRoutineResponse(
            true,
            PersonalRoutineRequestKind.Perform,
            PersonalRoutineFailureReason.None,
            message);
    }

    private bool TryGetCooldownFailure(
        uint playerEntityId,
        PersonalRoutineKind routine,
        PersonalRoutineDefinition definition,
        out PersonalRoutineResponse failure)
    {
        var key = (playerEntityId, routine);
        if (_cooldowns.TryGetValue(key, out var lastUsed) && DateTime.UtcNow - lastUsed < definition.Cooldown)
        {
            var remaining = definition.Cooldown - (DateTime.UtcNow - lastUsed);
            failure = Fail(
                PersonalRoutineRequestKind.Perform,
                PersonalRoutineFailureReason.OnCooldown,
                $"Please wait {remaining.TotalSeconds:F0}s before '{definition.CommandHint}' again.");
            return true;
        }

        failure = default;
        return false;
    }

    private void SetCooldown(uint playerEntityId, PersonalRoutineKind routine)
    {
        _cooldowns[(playerEntityId, routine)] = DateTime.UtcNow;
    }

    private static PersonalRoutineResponse Fail(
        PersonalRoutineRequestKind kind,
        PersonalRoutineFailureReason reason,
        string message)
    {
        Log.Information("Personal routine request {Kind} failed ({Reason}): {Message}", kind, reason, message);
        return new PersonalRoutineResponse(false, kind, reason, message);
    }
}