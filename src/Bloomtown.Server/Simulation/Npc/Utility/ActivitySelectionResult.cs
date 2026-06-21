using Bloomtown.Server.Simulation.Npc.Schedule;

namespace Bloomtown.Server.Simulation.Npc.Utility;

/// <summary>
/// Outcome of a utility evaluation, including hysteresis decision and logging context.
/// </summary>
public sealed class ActivitySelectionResult
{
    public required NpcActivityType SelectedActivity { get; init; }
    public required float SelectedScore { get; init; }
    public required ActivityScore SelectedBreakdown { get; init; }
    public NpcActivityType? RunnerUpActivity { get; init; }
    public float? RunnerUpScore { get; init; }
    public bool ActivityChanged { get; init; }
    public bool EvaluationPerformed { get; init; }

    public string BuildLogReason()
    {
        var factorSummary = SelectedBreakdown.DescribeTopFactors();

        if (RunnerUpActivity is null || RunnerUpScore is null)
            return factorSummary;

        return $"{factorSummary} (beat {RunnerUpActivity} at {RunnerUpScore:F0})";
    }
}