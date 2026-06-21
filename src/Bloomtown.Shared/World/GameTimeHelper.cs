using Bloomtown.Shared.Village;

namespace Bloomtown.Shared.World;

/// <summary>
/// Convenience helpers for game-clock time-of-day phases.
/// </summary>
public static class GameTimeHelper
{
    /// <summary>Maps game hour (0–23) to Morning / Afternoon / Evening / Night.</summary>
    public static GameTimeOfDay GetTimeOfDay(int gameHour) =>
        VillageLifeConfig.GetTimeOfDay(gameHour);

    public static string GetDisplayName(GameTimeOfDay timeOfDay) =>
        VillageLifeConfig.GetTimeOfDayDisplayName(timeOfDay);

    public static string FormatClock(int gameDay, int gameHour, int minuteOfHour) =>
        VillageLifeConfig.FormatGameTimeStatus(gameDay, gameHour, minuteOfHour);
}