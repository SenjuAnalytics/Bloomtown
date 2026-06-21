using Bloomtown.Server.Simulation.Community;
using Bloomtown.Server.Simulation.Economy;
using Bloomtown.Shared.Community;
using Bloomtown.Shared.Needs;
using Serilog;

namespace Bloomtown.Server.Simulation.Needs;

/// <summary>
/// Advances player Mood, Fatigue, and Social Need over game time; applies activity effects and penalties.
/// </summary>
public sealed class PlayerNeedsService : ISimulationSystem
{
    private readonly PlayerEconomyService _economyService;
    private readonly WorldTimeSystem _worldTime;
    private readonly VillageProjectStateService? _villageProjectState;
    private readonly double _gameMinutesPerRealSecond;
    private readonly HashSet<uint> _exhaustedWarned = new();
    private readonly HashSet<uint> _lonelyWarned = new();
    private readonly HashSet<uint> _lowMoodWarned = new();

    public PlayerNeedsService(
        PlayerEconomyService economyService,
        WorldTimeSystem worldTime,
        double gameMinutesPerRealSecond = 1.0,
        VillageProjectStateService? villageProjectState = null)
    {
        _economyService = economyService;
        _worldTime = worldTime;
        _villageProjectState = villageProjectState;
        _gameMinutesPerRealSecond = gameMinutesPerRealSecond;
    }

    /// <summary>
    /// Applies offline catch-up after economy state is loaded from persistence.
    /// </summary>
    public void OnPlayerConnected(uint playerEntityId)
    {
        if (!_economyService.TryGetState(playerEntityId, out var economy))
            return;

        var currentMinute = _worldTime.TotalGameMinutes;
        if (economy.LastNeedsUpdateTotalGameMinute > 0 && economy.LastNeedsUpdateTotalGameMinute < currentMinute)
        {
            var elapsed = currentMinute - economy.LastNeedsUpdateTotalGameMinute;
            ApplyDecay(economy, elapsed, GetDevelopmentBonuses());
            Log.Information(
                "Player {PlayerId} needs catch-up applied for {Elapsed} offline game minute(s): Mood {Mood:F0}, Fatigue {Fatigue:F0}, Social {Social:F0}.",
                playerEntityId,
                elapsed,
                economy.Mood,
                economy.Fatigue,
                economy.SocialNeed);
        }

        economy.LastNeedsUpdateTotalGameMinute = currentMinute;
    }

    public void ClearPlayer(uint playerEntityId)
    {
        _exhaustedWarned.Remove(playerEntityId);
        _lonelyWarned.Remove(playerEntityId);
        _lowMoodWarned.Remove(playerEntityId);
    }

    public void Update(double deltaTimeSeconds)
    {
        if (deltaTimeSeconds <= 0)
            return;

        var deltaGameMinutes = deltaTimeSeconds * _gameMinutesPerRealSecond;
        var currentMinute = _worldTime.TotalGameMinutes;

        foreach (var playerEntityId in _economyService.GetCachedPlayerIds())
        {
            if (!_economyService.TryGetState(playerEntityId, out var economy))
                continue;

            ApplyDecay(economy, deltaGameMinutes, GetDevelopmentBonuses());
            economy.LastNeedsUpdateTotalGameMinute = currentMinute;
            LogSignificantThresholds(playerEntityId, economy);
        }
    }

    private VillageDevelopmentBonuses GetDevelopmentBonuses() =>
        _villageProjectState?.GetActiveBonuses() ?? VillageDevelopmentBonuses.None;

    public void ApplyGift(PlayerEconomyState economy)
    {
        economy.Mood = PlayerNeedsConfig.Clamp(economy.Mood + PlayerNeedsConfig.GiftMoodGain);
        economy.SocialNeed = PlayerNeedsConfig.Clamp(economy.SocialNeed - PlayerNeedsConfig.GiftSocialReduction);
        LogActivityRecovery(economy.PlayerEntityId, "gift", economy);
    }

    public void ApplyTalk(PlayerEconomyState economy)
    {
        economy.SocialNeed = PlayerNeedsConfig.Clamp(economy.SocialNeed - PlayerNeedsConfig.TalkSocialReduction);
        LogActivityRecovery(economy.PlayerEntityId, "talk", economy);
    }

    public void ApplyGreet(PlayerEconomyState economy)
    {
        economy.SocialNeed = PlayerNeedsConfig.Clamp(economy.SocialNeed - PlayerNeedsConfig.GreetSocialReduction);
        LogActivityRecovery(economy.PlayerEntityId, "greet", economy);
    }

    /// <summary>
    /// Small mood/social lift when Elsie or Harold feel emotionally close to the player.
    /// Stacks on top of baseline greet/talk recovery — kept subtle so bonds feel valuable, not overpowered.
    /// </summary>
    public void ApplyEmotionalBondRecovery(PlayerEconomyState economy, float moodBonus, float socialBonus)
    {
        if (moodBonus <= 0f && socialBonus <= 0f)
            return;

        var moodBefore = economy.Mood;
        var socialBefore = economy.SocialNeed;

        if (moodBonus > 0f)
            economy.Mood = PlayerNeedsConfig.Clamp(economy.Mood + moodBonus);

        if (socialBonus > 0f)
            economy.SocialNeed = PlayerNeedsConfig.Clamp(economy.SocialNeed - socialBonus);

        _lonelyWarned.Remove(economy.PlayerEntityId);
        _lowMoodWarned.Remove(economy.PlayerEntityId);

        Log.Information(
            "Player {PlayerId} emotional bond recovery — mood {MoodBefore:F0}->{MoodAfter:F0} (+{MoodBonus:F0}), social {SocialBefore:F0}->{SocialAfter:F0} (-{SocialBonus:F0}).",
            economy.PlayerEntityId,
            moodBefore,
            economy.Mood,
            moodBonus,
            socialBefore,
            economy.SocialNeed,
            socialBonus);
    }

    /// <summary>Extra mood/social lift when the village treats the player as a familiar face.</summary>
    public void ApplySocialInteractionBonus(PlayerEconomyState economy, float moodBonus, float socialBonus)
    {
        if (moodBonus <= 0f && socialBonus <= 0f)
            return;

        var moodBefore = economy.Mood;
        var socialBefore = economy.SocialNeed;

        if (moodBonus > 0f)
            economy.Mood = PlayerNeedsConfig.Clamp(economy.Mood + moodBonus);

        if (socialBonus > 0f)
            economy.SocialNeed = PlayerNeedsConfig.Clamp(economy.SocialNeed - socialBonus);

        _lonelyWarned.Remove(economy.PlayerEntityId);
        _lowMoodWarned.Remove(economy.PlayerEntityId);

        Log.Information(
            "Player {PlayerId} social interaction bonus — mood {MoodBefore:F0}->{MoodAfter:F0} (+{MoodBonus:F0}), social {SocialBefore:F0}->{SocialAfter:F0} (-{SocialBonus:F0}).",
            economy.PlayerEntityId,
            moodBefore,
            economy.Mood,
            moodBonus,
            socialBefore,
            economy.SocialNeed,
            socialBonus);
    }

    public void ApplyRest(PlayerEconomyState economy)
    {
        var fatigueBefore = economy.Fatigue;
        var moodBefore = economy.Mood;
        economy.Fatigue = PlayerNeedsConfig.Clamp(economy.Fatigue - PlayerNeedsConfig.RestFatigueReduction);
        economy.Mood = PlayerNeedsConfig.Clamp(economy.Mood + PlayerNeedsConfig.RestMoodGain);
        _exhaustedWarned.Remove(economy.PlayerEntityId);

        Log.Information(
            "Player {PlayerId} rested outdoors — mood {MoodBefore:F0}->{MoodAfter:F0}, fatigue {FatigueBefore:F0}->{FatigueAfter:F0}.",
            economy.PlayerEntityId,
            moodBefore,
            economy.Mood,
            fatigueBefore,
            economy.Fatigue);
    }

    /// <summary>
    /// Cozy at-home rituals restore more Mood and Fatigue than outdoor rest.
    /// </summary>
    public void ApplyHomeActivity(PlayerEconomyState economy, float moodGain, float fatigueReduction)
    {
        var moodBefore = economy.Mood;
        var fatigueBefore = economy.Fatigue;
        economy.Mood = PlayerNeedsConfig.Clamp(economy.Mood + moodGain);
        economy.Fatigue = PlayerNeedsConfig.Clamp(economy.Fatigue - fatigueReduction);
        _exhaustedWarned.Remove(economy.PlayerEntityId);
        _lowMoodWarned.Remove(economy.PlayerEntityId);

        Log.Information(
            "Player {PlayerId} cozy home activity — mood {MoodBefore:F0}->{MoodAfter:F0} (+{MoodGain:F0}), fatigue {FatigueBefore:F0}->{FatigueAfter:F0} (-{FatigueDrop:F0}).",
            economy.PlayerEntityId,
            moodBefore,
            economy.Mood,
            economy.Mood - moodBefore,
            fatigueBefore,
            economy.Fatigue,
            fatigueBefore - economy.Fatigue);
    }

    public void ApplySleep(PlayerEconomyState economy, int comfortScore)
    {
        var fatigueBefore = economy.Fatigue;
        var moodBefore = economy.Mood;
        economy.Fatigue = PlayerNeedsConfig.Clamp(economy.Fatigue - PlayerNeedsConfig.SleepFatigueReduction);
        economy.Mood = PlayerNeedsConfig.Clamp(
            economy.Mood + PlayerNeedsConfig.GetSleepMoodGain(comfortScore));
        _exhaustedWarned.Remove(economy.PlayerEntityId);
        _lowMoodWarned.Remove(economy.PlayerEntityId);

        Log.Information(
            "Player {PlayerId} slept — fatigue {FatigueBefore:F0}->{FatigueAfter:F0}, mood {MoodBefore:F0}->{MoodAfter:F0} (comfort {Comfort}).",
            economy.PlayerEntityId,
            fatigueBefore,
            economy.Fatigue,
            moodBefore,
            economy.Mood,
            comfortScore);
    }

    public void ApplyGatherStart(PlayerEconomyState economy)
    {
        economy.Fatigue = PlayerNeedsConfig.Clamp(economy.Fatigue + PlayerNeedsConfig.GatherStartFatigueGain);
    }

    public void ApplyGatherComplete(PlayerEconomyState economy)
    {
        economy.Fatigue = PlayerNeedsConfig.Clamp(economy.Fatigue + PlayerNeedsConfig.GatherCompleteFatigueGain);
        LogSignificantThresholds(economy.PlayerEntityId, economy);
    }

    public void ApplyHouseUpgrade(PlayerEconomyState economy)
    {
        economy.Mood = PlayerNeedsConfig.Clamp(economy.Mood + PlayerNeedsConfig.HouseUpgradeMoodGain);
        _lowMoodWarned.Remove(economy.PlayerEntityId);

        Log.Information(
            "Player {PlayerId} home upgrade boosted mood to {Mood:F0}.",
            economy.PlayerEntityId,
            economy.Mood);
    }

    public void ApplyPlaceFurniture(PlayerEconomyState economy)
    {
        economy.Mood = PlayerNeedsConfig.Clamp(economy.Mood + PlayerNeedsConfig.PlaceFurnitureMoodGain);
        _lowMoodWarned.Remove(economy.PlayerEntityId);

        Log.Information(
            "Player {PlayerId} placed furniture — mood now {Mood:F0}.",
            economy.PlayerEntityId,
            economy.Mood);
    }

    public double GetGatherDurationMultiplier(PlayerEconomyState economy)
    {
        return PlayerNeedsConfig.GetGatherDurationMultiplier(economy.Mood, economy.Fatigue);
    }

    /// <summary>
    /// Community-help recovery — mood lift plus stronger Social Need relief than personal leisure.
    /// </summary>
    public void ApplyCommunityActivity(PlayerEconomyState economy, float moodGain, float socialReduction)
    {
        if (moodGain <= 0f && socialReduction <= 0f)
            return;

        var moodBefore = economy.Mood;
        var socialBefore = economy.SocialNeed;

        if (moodGain > 0f)
            economy.Mood = PlayerNeedsConfig.Clamp(economy.Mood + moodGain);

        if (socialReduction > 0f)
            economy.SocialNeed = PlayerNeedsConfig.Clamp(economy.SocialNeed - socialReduction);

        _lonelyWarned.Remove(economy.PlayerEntityId);
        _lowMoodWarned.Remove(economy.PlayerEntityId);

        Log.Information(
            "Player {PlayerId} community help — mood {MoodBefore:F0}->{MoodAfter:F0} (+{MoodGain:F0}), social {SocialBefore:F0}->{SocialAfter:F0} (-{SocialDrop:F0}).",
            economy.PlayerEntityId,
            moodBefore,
            economy.Mood,
            economy.Mood - moodBefore,
            socialBefore,
            economy.SocialNeed,
            socialBefore - economy.SocialNeed);
    }

    /// <summary>Light passive bonus from the player's daily rhythm pattern.</summary>
    public void ApplyDailyRhythmBonus(
        PlayerEconomyState economy,
        float moodGain,
        float fatigueReduction,
        float socialReduction)
    {
        if (moodGain <= 0f && fatigueReduction <= 0f && socialReduction <= 0f)
            return;

        var moodBefore = economy.Mood;
        var fatigueBefore = economy.Fatigue;
        var socialBefore = economy.SocialNeed;

        if (moodGain > 0f)
            economy.Mood = PlayerNeedsConfig.Clamp(economy.Mood + moodGain);

        if (fatigueReduction > 0f)
            economy.Fatigue = PlayerNeedsConfig.Clamp(economy.Fatigue - fatigueReduction);

        if (socialReduction > 0f)
            economy.SocialNeed = PlayerNeedsConfig.Clamp(economy.SocialNeed - socialReduction);

        _exhaustedWarned.Remove(economy.PlayerEntityId);
        _lonelyWarned.Remove(economy.PlayerEntityId);
        _lowMoodWarned.Remove(economy.PlayerEntityId);

        Log.Information(
            "Player {PlayerId} daily rhythm bonus — mood {MoodBefore:F0}->{MoodAfter:F0} (+{MoodGain:F0}), fatigue {FatigueBefore:F0}->{FatigueAfter:F0} (-{FatigueDrop:F0}), social {SocialBefore:F0}->{SocialAfter:F0} (-{SocialDrop:F0}).",
            economy.PlayerEntityId,
            moodBefore,
            economy.Mood,
            moodGain,
            fatigueBefore,
            economy.Fatigue,
            fatigueReduction,
            socialBefore,
            economy.SocialNeed,
            socialReduction);
    }

    /// <summary>
    /// Daily village leisure — balanced mood, fatigue, and social recovery lighter than community help.
    /// </summary>
    public void ApplyDailyVillageActivity(
        PlayerEconomyState economy,
        float moodGain,
        float fatigueReduction,
        float socialReduction)
    {
        if (moodGain <= 0f && fatigueReduction <= 0f && socialReduction <= 0f)
            return;

        var moodBefore = economy.Mood;
        var fatigueBefore = economy.Fatigue;
        var socialBefore = economy.SocialNeed;

        if (moodGain > 0f)
            economy.Mood = PlayerNeedsConfig.Clamp(economy.Mood + moodGain);

        if (fatigueReduction > 0f)
            economy.Fatigue = PlayerNeedsConfig.Clamp(economy.Fatigue - fatigueReduction);

        if (socialReduction > 0f)
            economy.SocialNeed = PlayerNeedsConfig.Clamp(economy.SocialNeed - socialReduction);

        _exhaustedWarned.Remove(economy.PlayerEntityId);
        _lonelyWarned.Remove(economy.PlayerEntityId);
        _lowMoodWarned.Remove(economy.PlayerEntityId);

        Log.Information(
            "Player {PlayerId} daily village leisure — mood {MoodBefore:F0}->{MoodAfter:F0} (+{MoodGain:F0}), fatigue {FatigueBefore:F0}->{FatigueAfter:F0} (-{FatigueDrop:F0}), social {SocialBefore:F0}->{SocialAfter:F0} (-{SocialDrop:F0}).",
            economy.PlayerEntityId,
            moodBefore,
            economy.Mood,
            moodGain,
            fatigueBefore,
            economy.Fatigue,
            fatigueReduction,
            socialBefore,
            economy.SocialNeed,
            socialReduction);
    }

    /// <summary>Personal routine recovery — gentle Mood/Fatigue lift from daily rituals.</summary>
    public void ApplyPersonalRoutine(PlayerEconomyState economy, float moodGain, float fatigueReduction)
    {
        if (moodGain <= 0f && fatigueReduction <= 0f)
            return;

        var moodBefore = economy.Mood;
        var fatigueBefore = economy.Fatigue;

        if (moodGain > 0f)
            economy.Mood = PlayerNeedsConfig.Clamp(economy.Mood + moodGain);

        if (fatigueReduction > 0f)
            economy.Fatigue = PlayerNeedsConfig.Clamp(economy.Fatigue - fatigueReduction);

        _exhaustedWarned.Remove(economy.PlayerEntityId);
        _lowMoodWarned.Remove(economy.PlayerEntityId);

        Log.Information(
            "Player {PlayerId} personal routine — mood {MoodBefore:F0}->{MoodAfter:F0} (+{MoodGain:F0}), fatigue {FatigueBefore:F0}->{FatigueAfter:F0} (-{FatigueDrop:F0}).",
            economy.PlayerEntityId,
            moodBefore,
            economy.Mood,
            economy.Mood - moodBefore,
            fatigueBefore,
            economy.Fatigue,
            fatigueBefore - economy.Fatigue);
    }

    /// <summary>
    /// Very light passive mood lift when the village regards the player's close focus bonds.
    /// Only applied while the player is out in town — kept subtle so it feels like belonging, not a buff.
    /// </summary>
    public void ApplyVillageBondRecognitionPassiveRelief(uint playerEntityId, float moodGain)
    {
        if (moodGain <= 0f)
            return;

        if (!_economyService.TryGetState(playerEntityId, out var economy))
            return;

        var moodBefore = economy.Mood;
        economy.Mood = PlayerNeedsConfig.Clamp(economy.Mood + moodGain);

        if (economy.Mood <= moodBefore)
            return;

        _lowMoodWarned.Remove(playerEntityId);
        Log.Debug(
            "Player {PlayerId} village bond recognition passive — mood {MoodBefore:F2}->{MoodAfter:F2} (+{MoodGain:F3}).",
            playerEntityId,
            moodBefore,
            economy.Mood,
            moodGain);
    }

    /// <summary>Passive benefit while lingering inside an unlocked village area.</summary>
    public void ApplyAreaPassiveRelief(uint playerEntityId, float moodGain, float fatigueReduction)
    {
        if (moodGain <= 0f && fatigueReduction <= 0f)
            return;

        if (!_economyService.TryGetState(playerEntityId, out var economy))
            return;

        var moodBefore = economy.Mood;
        var fatigueBefore = economy.Fatigue;

        if (moodGain > 0f)
            economy.Mood = PlayerNeedsConfig.Clamp(economy.Mood + moodGain);

        if (fatigueReduction > 0f)
            economy.Fatigue = PlayerNeedsConfig.Clamp(economy.Fatigue - fatigueReduction);

        if (economy.Mood <= moodBefore && economy.Fatigue >= fatigueBefore)
            return;

        _exhaustedWarned.Remove(playerEntityId);
        _lowMoodWarned.Remove(playerEntityId);
        Log.Debug(
            "Player {PlayerId} village area passive — mood {MoodBefore:F1}->{MoodAfter:F1}, fatigue {FatigueBefore:F1}->{FatigueAfter:F1}.",
            playerEntityId,
            moodBefore,
            economy.Mood,
            fatigueBefore,
            economy.Fatigue);
    }

    /// <summary>Passive benefit while standing near the repaired bridge.</summary>
    public void ApplyBridgePassiveRelief(uint playerEntityId, float fatigueReduction)
    {
        if (fatigueReduction <= 0)
            return;

        if (!_economyService.TryGetState(playerEntityId, out var economy))
            return;

        var before = economy.Fatigue;
        economy.Fatigue = PlayerNeedsConfig.Clamp(economy.Fatigue - fatigueReduction);
        if (economy.Fatigue >= before)
            return;

        _exhaustedWarned.Remove(playerEntityId);
        Log.Debug(
            "Player {PlayerId} bridge passive relief — fatigue {FatigueBefore:F1}->{FatigueAfter:F1}.",
            playerEntityId,
            before,
            economy.Fatigue);
    }

    /// <summary>Passive benefit when crossing the repaired bridge.</summary>
    public float ApplyBridgeCrossRelief(PlayerEconomyState economy, float fatigueReduction)
    {
        if (fatigueReduction <= 0)
            return 0;

        var before = economy.Fatigue;
        economy.Fatigue = PlayerNeedsConfig.Clamp(economy.Fatigue - fatigueReduction);
        _exhaustedWarned.Remove(economy.PlayerEntityId);

        var reduced = before - economy.Fatigue;
        if (reduced > 0)
        {
            Log.Information(
                "Player {PlayerId} bridge cross eased fatigue {FatigueBefore:F0}->{FatigueAfter:F0} (-{Reduced:F0}).",
                economy.PlayerEntityId,
                before,
                economy.Fatigue,
                reduced);
        }

        return reduced;
    }

    /// <summary>
    /// Passive decay: fatigue and social need rise; mood falls when stressed.
    /// Village development bonuses gently ease fatigue rise and support mood recovery.
    /// </summary>
    private static void ApplyDecay(
        PlayerEconomyState economy,
        double deltaGameMinutes,
        VillageDevelopmentBonuses villageBonuses)
    {
        if (deltaGameMinutes <= 0)
            return;

        var minutes = (float)deltaGameMinutes;
        var stressed = PlayerNeedsConfig.IsUnderStress(economy.Fatigue, economy.SocialNeed, economy.Hunger);

        // Village bonus: fatigue rises more slowly in a Lively or Bustling village.
        economy.Fatigue = PlayerNeedsConfig.Clamp(
            economy.Fatigue
            + PlayerNeedsConfig.FatigueRisePerGameMinute * villageBonuses.FatigueRiseMultiplier * minutes);

        // Social need rises over time — higher means lonelier.
        economy.SocialNeed = PlayerNeedsConfig.Clamp(
            economy.SocialNeed + PlayerNeedsConfig.SocialNeedRisePerGameMinute * minutes);

        if (stressed)
        {
            // Village bonus (Bustling): mood decays slightly slower under stress.
            economy.Mood = PlayerNeedsConfig.Clamp(
                economy.Mood
                - PlayerNeedsConfig.MoodDecayUnderStressPerGameMinute
                  * villageBonuses.MoodDecayUnderStressMultiplier
                  * minutes);
        }
        else if (villageBonuses.PassiveMoodRecoveryPerGameMinute > 0f)
        {
            // Village bonus (Lively+): gentle mood recovery when life feels manageable.
            economy.Mood = PlayerNeedsConfig.Clamp(
                economy.Mood + villageBonuses.PassiveMoodRecoveryPerGameMinute * minutes);
        }
    }

    private void LogSignificantThresholds(uint playerEntityId, PlayerEconomyState economy)
    {
        if (!PlayerNeedsConfig.IsExhausted(economy.Fatigue))
            _exhaustedWarned.Remove(playerEntityId);
        else if (_exhaustedWarned.Add(playerEntityId))
        {
            Log.Information(
                "Player {PlayerId} fatigue reached exhausted level ({Fatigue:F0}/{Max:F0}) — gathering penalty active.",
                playerEntityId,
                economy.Fatigue,
                PlayerNeedsConfig.MaxValue);
        }

        if (!PlayerNeedsConfig.IsLonely(economy.SocialNeed))
            _lonelyWarned.Remove(playerEntityId);
        else if (_lonelyWarned.Add(playerEntityId))
        {
            Log.Information(
                "Player {PlayerId} social need reached lonely level ({Social:F0}/{Max:F0}).",
                playerEntityId,
                economy.SocialNeed,
                PlayerNeedsConfig.MaxValue);
        }

        if (!PlayerNeedsConfig.IsLowMood(economy.Mood))
            _lowMoodWarned.Remove(playerEntityId);
        else if (_lowMoodWarned.Add(playerEntityId))
        {
            Log.Information(
                "Player {PlayerId} mood dropped to low level ({Mood:F0}/{Max:F0}) — small gathering penalty active.",
                playerEntityId,
                economy.Mood,
                PlayerNeedsConfig.MaxValue);
        }
    }

    private static void LogActivityRecovery(uint playerEntityId, string activity, PlayerEconomyState economy)
    {
        Log.Debug(
            "Player {PlayerId} {Activity} — needs now Mood {Mood:F0}, Fatigue {Fatigue:F0}, Social {Social:F0}.",
            playerEntityId,
            activity,
            economy.Mood,
            economy.Fatigue,
            economy.SocialNeed);
    }
}