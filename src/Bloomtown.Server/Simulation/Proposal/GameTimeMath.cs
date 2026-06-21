namespace Bloomtown.Server.Simulation.Proposal;

/// <summary>
/// Converts between game day/minute and a linear minute index for voting deadlines.
/// </summary>
public static class GameTimeMath
{
    public const int MinutesPerDay = 24 * 60;

    public static long ToTotalGameMinutes(int gameDay, int gameMinute)
    {
        return (long)(gameDay - 1) * MinutesPerDay + gameMinute;
    }

    public static (int GameDay, int GameMinute) FromTotalGameMinutes(long totalMinutes)
    {
        if (totalMinutes < 0)
            totalMinutes = 0;

        var gameDay = (int)(totalMinutes / MinutesPerDay) + 1;
        var gameMinute = (int)(totalMinutes % MinutesPerDay);
        return (gameDay, gameMinute);
    }

    public static long AddGameMinutes(int gameDay, int gameMinute, int minutesToAdd)
    {
        return ToTotalGameMinutes(gameDay, gameMinute) + minutesToAdd;
    }

    public static int MinutesRemaining(long currentTotalMinutes, long endTotalMinutes)
    {
        return (int)Math.Max(0, endTotalMinutes - currentTotalMinutes);
    }
}