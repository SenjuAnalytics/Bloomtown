using Bloomtown.Shared.World;

namespace Bloomtown.Server.Simulation;

/// <summary>
/// Advances the in-game clock. One real second equals one game minute by default.
/// </summary>
public sealed class WorldTimeSystem : ISimulationSystem
{
    public const int MinutesPerHour = 60;
    public const int MinutesPerDay = 24 * MinutesPerHour;

    private readonly double _gameMinutesPerRealSecond;
    private double _fractionalMinuteAccumulator;

    public int GameDay { get; private set; } = 1;

    /// <summary>Minutes elapsed since midnight on the current game day (0–1439).</summary>
    public int GameMinute { get; private set; }

    public int GameHour => GameMinute / MinutesPerHour;

    public int MinuteOfHour => GameMinute % MinutesPerHour;

    /// <summary>Current broad time band (Morning / Afternoon / Evening / Night).</summary>
    public GameTimeOfDay CurrentTimeOfDay => GameTimeHelper.GetTimeOfDay(GameHour);

    /// <summary>Total game minutes elapsed since world start (used for offline needs catch-up).</summary>
    public long TotalGameMinutes => (long)(GameDay - 1) * MinutesPerDay + GameMinute;

    /// <summary>Fired when the in-game hour changes (0–23).</summary>
    public event Action<int>? OnHourAdvanced;

    /// <summary>Fired when a new game day begins. Argument is the new day number.</summary>
    public event Action<int>? OnDayRollover;

    public WorldTimeSystem(double gameMinutesPerRealSecond = 1.0)
    {
        if (gameMinutesPerRealSecond <= 0)
            throw new ArgumentOutOfRangeException(nameof(gameMinutesPerRealSecond));

        _gameMinutesPerRealSecond = gameMinutesPerRealSecond;
    }

    /// <summary>Restores world time from persistence. Fractional minute progress resets to 0.</summary>
    public void LoadState(int gameDay, int gameMinute)
    {
        if (gameDay < 1)
            throw new ArgumentOutOfRangeException(nameof(gameDay));

        if (gameMinute is < 0 or >= MinutesPerDay)
            throw new ArgumentOutOfRangeException(nameof(gameMinute));

        GameDay = gameDay;
        GameMinute = gameMinute;
        _fractionalMinuteAccumulator = 0;
    }

    public void Update(double deltaTimeSeconds)
    {
        if (deltaTimeSeconds <= 0)
            return;

        _fractionalMinuteAccumulator += deltaTimeSeconds * _gameMinutesPerRealSecond;

        // Convert accumulated fractional time into whole game-minute steps.
        while (_fractionalMinuteAccumulator >= 1.0)
        {
            _fractionalMinuteAccumulator -= 1.0;
            AdvanceOneGameMinute();
        }
    }

    private void AdvanceOneGameMinute()
    {
        var previousHour = GameHour;
        GameMinute++;

        if (GameMinute < MinutesPerDay)
        {
            if (GameHour != previousHour)
                OnHourAdvanced?.Invoke(GameHour);

            return;
        }

        // Midnight rollover: start a new game day.
        GameMinute = 0;
        GameDay++;
        OnDayRollover?.Invoke(GameDay);
        OnHourAdvanced?.Invoke(0);
    }
}