namespace Bloomtown.Server.Simulation.Npc.Schedule;

/// <summary>
/// One daily schedule segment using in-game minutes since midnight [start, end).
/// </summary>
public readonly record struct ScheduleBlock(int StartMinute, int EndMinute, NpcActivityType Activity)
{
    public bool Contains(int gameMinute)
    {
        return gameMinute >= StartMinute && gameMinute < EndMinute;
    }
}