namespace Bloomtown.Server.Simulation.Npc.Schedule;

/// <summary>
/// Simple daily activity template keyed by world clock.
/// </summary>
public sealed class NpcDailySchedule
{
    private readonly IReadOnlyList<ScheduleBlock> _blocks;

    public NpcDailySchedule(IEnumerable<ScheduleBlock> blocks)
    {
        _blocks = blocks.OrderBy(block => block.StartMinute).ToList();
        if (_blocks.Count == 0)
            throw new ArgumentException("Schedule requires at least one block.", nameof(blocks));
    }

    public NpcActivityType GetActivityAt(int gameMinute)
    {
        foreach (var block in _blocks)
        {
            if (block.Contains(gameMinute))
                return block.Activity;
        }

        return NpcActivityType.Rest;
    }

    /// <summary>
    /// Returns a 0–100 fit score for how appropriate an activity is at the given game minute.
    /// </summary>
    public float GetScheduleFit(NpcActivityType activity, int gameMinute)
    {
        var scheduled = GetActivityAt(gameMinute);

        if (activity == scheduled)
            return 100f;

        // Social overlaps with patrol blocks; Eat/Rest blocks tolerate urgent need overrides.
        if (activity == NpcActivityType.Social && scheduled == NpcActivityType.Patrol)
            return 70f;

        if (activity == NpcActivityType.Eat && scheduled == NpcActivityType.Work)
            return 35f;

        if (activity == NpcActivityType.Rest && scheduled == NpcActivityType.Work)
            return 30f;

        return 15f;
    }

    public static NpcDailySchedule CreateElsieSchedule()
    {
        return new NpcDailySchedule(
        [
            new ScheduleBlock(6 * 60, 12 * 60, NpcActivityType.Work),
            new ScheduleBlock(12 * 60, 14 * 60, NpcActivityType.Eat),
            new ScheduleBlock(14 * 60, 18 * 60, NpcActivityType.Patrol),
            new ScheduleBlock(18 * 60, 24 * 60, NpcActivityType.Rest),
        ]);
    }

    public static NpcDailySchedule CreateTomSchedule()
    {
        return new NpcDailySchedule(
        [
            new ScheduleBlock(7 * 60, 13 * 60, NpcActivityType.Work),
            new ScheduleBlock(13 * 60, 15 * 60, NpcActivityType.Eat),
            new ScheduleBlock(15 * 60, 19 * 60, NpcActivityType.Patrol),
            new ScheduleBlock(19 * 60, 24 * 60, NpcActivityType.Rest),
        ]);
    }

    /// <summary>Market merchant — busy at the square through trade hours.</summary>
    public static NpcDailySchedule CreateMiraSchedule()
    {
        return new NpcDailySchedule(
        [
            new ScheduleBlock(8 * 60, 12 * 60, NpcActivityType.Work),
            new ScheduleBlock(12 * 60, 13 * 60, NpcActivityType.Eat),
            new ScheduleBlock(13 * 60, 17 * 60, NpcActivityType.Social),
            new ScheduleBlock(17 * 60, 20 * 60, NpcActivityType.Work),
            new ScheduleBlock(20 * 60, 24 * 60, NpcActivityType.Rest),
        ]);
    }

    /// <summary>Village elder — unhurried patrol and porch-side social time.</summary>
    public static NpcDailySchedule CreateHaroldSchedule()
    {
        return new NpcDailySchedule(
        [
            new ScheduleBlock(7 * 60, 11 * 60, NpcActivityType.Patrol),
            new ScheduleBlock(11 * 60, 13 * 60, NpcActivityType.Social),
            new ScheduleBlock(13 * 60, 14 * 60, NpcActivityType.Eat),
            new ScheduleBlock(14 * 60, 18 * 60, NpcActivityType.Patrol),
            new ScheduleBlock(18 * 60, 24 * 60, NpcActivityType.Rest),
        ]);
    }

    /// <summary>Innkeeper — busy at the hearth through meal and guest hours.</summary>
    public static NpcDailySchedule CreateGretaSchedule()
    {
        return new NpcDailySchedule(
        [
            new ScheduleBlock(7 * 60, 11 * 60, NpcActivityType.Work),
            new ScheduleBlock(11 * 60, 13 * 60, NpcActivityType.Eat),
            new ScheduleBlock(13 * 60, 17 * 60, NpcActivityType.Social),
            new ScheduleBlock(17 * 60, 21 * 60, NpcActivityType.Work),
            new ScheduleBlock(21 * 60, 24 * 60, NpcActivityType.Rest),
        ]);
    }

    /// <summary>Herbalist — calm tending through morning and afternoon, quiet rest by evening.</summary>
    public static NpcDailySchedule CreateNoraSchedule()
    {
        return new NpcDailySchedule(
        [
            new ScheduleBlock(7 * 60, 11 * 60, NpcActivityType.Work),
            new ScheduleBlock(11 * 60, 13 * 60, NpcActivityType.Eat),
            new ScheduleBlock(13 * 60, 17 * 60, NpcActivityType.Work),
            new ScheduleBlock(17 * 60, 19 * 60, NpcActivityType.Social),
            new ScheduleBlock(19 * 60, 24 * 60, NpcActivityType.Rest),
        ]);
    }

    /// <summary>Blacksmith — steady forge work through trade hours, quiet rest by evening.</summary>
    public static NpcDailySchedule CreateEliasSchedule()
    {
        return new NpcDailySchedule(
        [
            new ScheduleBlock(7 * 60, 12 * 60, NpcActivityType.Work),
            new ScheduleBlock(12 * 60, 13 * 60, NpcActivityType.Eat),
            new ScheduleBlock(13 * 60, 18 * 60, NpcActivityType.Work),
            new ScheduleBlock(18 * 60, 20 * 60, NpcActivityType.Social),
            new ScheduleBlock(20 * 60, 24 * 60, NpcActivityType.Rest),
        ]);
    }

    /// <summary>Craftsman — steady workshop work through trade hours, quiet rest by evening.</summary>
    public static NpcDailySchedule CreateMarcusSchedule()
    {
        return new NpcDailySchedule(
        [
            new ScheduleBlock(7 * 60, 12 * 60, NpcActivityType.Work),
            new ScheduleBlock(12 * 60, 13 * 60, NpcActivityType.Eat),
            new ScheduleBlock(13 * 60, 18 * 60, NpcActivityType.Work),
            new ScheduleBlock(18 * 60, 20 * 60, NpcActivityType.Social),
            new ScheduleBlock(20 * 60, 24 * 60, NpcActivityType.Rest),
        ]);
    }

    /// <summary>Village guard — patrol through morning and afternoon, brief rest by evening.</summary>
    public static NpcDailySchedule CreateBenSchedule()
    {
        return new NpcDailySchedule(
        [
            new ScheduleBlock(6 * 60, 12 * 60, NpcActivityType.Patrol),
            new ScheduleBlock(12 * 60, 13 * 60, NpcActivityType.Eat),
            new ScheduleBlock(13 * 60, 18 * 60, NpcActivityType.Patrol),
            new ScheduleBlock(18 * 60, 20 * 60, NpcActivityType.Social),
            new ScheduleBlock(20 * 60, 24 * 60, NpcActivityType.Rest),
        ]);
    }

    /// <summary>Young villager — social mornings, patrols the lanes, rests by evening.</summary>
    public static NpcDailySchedule CreateLilaSchedule()
    {
        return new NpcDailySchedule(
        [
            new ScheduleBlock(8 * 60, 12 * 60, NpcActivityType.Social),
            new ScheduleBlock(12 * 60, 13 * 60, NpcActivityType.Eat),
            new ScheduleBlock(13 * 60, 17 * 60, NpcActivityType.Patrol),
            new ScheduleBlock(17 * 60, 19 * 60, NpcActivityType.Social),
            new ScheduleBlock(19 * 60, 24 * 60, NpcActivityType.Rest),
        ]);
    }

    /// <summary>Storyteller — social and work mornings, quiet rest evenings.</summary>
    public static NpcDailySchedule CreateRowanSchedule()
    {
        return new NpcDailySchedule(
        [
            new ScheduleBlock(8 * 60, 12 * 60, NpcActivityType.Social),
            new ScheduleBlock(12 * 60, 13 * 60, NpcActivityType.Eat),
            new ScheduleBlock(13 * 60, 17 * 60, NpcActivityType.Work),
            new ScheduleBlock(17 * 60, 19 * 60, NpcActivityType.Social),
            new ScheduleBlock(19 * 60, 24 * 60, NpcActivityType.Rest),
        ]);
    }

    /// <summary>Retired teacher — porch social mornings, quiet rest afternoons, gentle evening company.</summary>
    public static NpcDailySchedule CreateEleanorSchedule()
    {
        return new NpcDailySchedule(
        [
            new ScheduleBlock(8 * 60, 12 * 60, NpcActivityType.Social),
            new ScheduleBlock(12 * 60, 13 * 60, NpcActivityType.Eat),
            new ScheduleBlock(13 * 60, 17 * 60, NpcActivityType.Rest),
            new ScheduleBlock(17 * 60, 19 * 60, NpcActivityType.Social),
            new ScheduleBlock(19 * 60, 24 * 60, NpcActivityType.Rest),
        ]);
    }
}