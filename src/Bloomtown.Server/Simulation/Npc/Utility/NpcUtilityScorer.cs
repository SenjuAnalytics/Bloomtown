using Bloomtown.Server.Simulation.Npc.Schedule;

namespace Bloomtown.Server.Simulation.Npc.Utility;

/// <summary>
/// Computes weighted utility scores and applies hysteresis so NPCs do not thrash between activities.
/// </summary>
public sealed class NpcUtilityScorer
{
    private readonly IReadOnlyList<IUtilityFactor> _factors;

    public NpcUtilityScorer()
        : this(
        [
            new NeedUrgencyFactor(),
            new ScheduleFitFactor(),
            new DistanceFactor(),
            new PersonalityFactor(),
        ])
    {
    }

    public NpcUtilityScorer(IEnumerable<IUtilityFactor> factors)
    {
        _factors = factors.ToList();
    }

    public IReadOnlyList<ActivityScore> ScoreAllActivities(in UtilityEvaluationContext context)
    {
        var scores = new List<ActivityScore>(UtilityScoringConfig.AvailableActivities.Length);

        foreach (var activity in UtilityScoringConfig.AvailableActivities)
            scores.Add(ScoreActivity(activity, in context));

        return scores.OrderByDescending(score => score.TotalScore).ToList();
    }

    /// <summary>
    /// Evaluates activities and decides whether to keep or switch the current activity.
    /// Hysteresis: switch only when the challenger wins by margin, or after minimum dwell time.
    /// </summary>
    public ActivitySelectionResult Evaluate(
        in UtilityEvaluationContext context,
        NpcActivityType currentActivity,
        double minutesInCurrentActivity,
        bool shouldEvaluate)
    {
        var rankedScores = ScoreAllActivities(in context);
        var best = rankedScores[0];
        var currentBreakdown = rankedScores.FirstOrDefault(score => score.Activity == currentActivity)
            ?? ScoreActivity(currentActivity, in context);

        if (!shouldEvaluate)
        {
            return new ActivitySelectionResult
            {
                SelectedActivity = currentActivity,
                SelectedScore = currentBreakdown.TotalScore,
                SelectedBreakdown = currentBreakdown,
                RunnerUpActivity = GetRunnerUp(rankedScores, currentActivity)?.Activity,
                RunnerUpScore = GetRunnerUp(rankedScores, currentActivity)?.TotalScore,
                ActivityChanged = false,
                EvaluationPerformed = false,
            };
        }

        var runnerUp = GetRunnerUp(rankedScores, best.Activity);
        var shouldSwitch = ShouldSwitchActivity(
            currentActivity,
            currentBreakdown.TotalScore,
            best.Activity,
            best.TotalScore,
            minutesInCurrentActivity);

        var selectedActivity = shouldSwitch ? best.Activity : currentActivity;
        var selectedBreakdown = shouldSwitch ? best : currentBreakdown;

        return new ActivitySelectionResult
        {
            SelectedActivity = selectedActivity,
            SelectedScore = selectedBreakdown.TotalScore,
            SelectedBreakdown = selectedBreakdown,
            RunnerUpActivity = shouldSwitch
                ? currentActivity
                : runnerUp?.Activity,
            RunnerUpScore = shouldSwitch
                ? currentBreakdown.TotalScore
                : runnerUp?.TotalScore,
            ActivityChanged = shouldSwitch && selectedActivity != currentActivity,
            EvaluationPerformed = true,
        };
    }

    private ActivityScore ScoreActivity(NpcActivityType activity, in UtilityEvaluationContext context)
    {
        var needUrgency = GetFactorScore<NeedUrgencyFactor>(activity, in context);
        var scheduleFit = GetFactorScore<ScheduleFitFactor>(activity, in context);
        var distance = GetFactorScore<DistanceFactor>(activity, in context);
        var personality = GetFactorScore<PersonalityFactor>(activity, in context);

        var total =
            needUrgency * UtilityScoringConfig.NeedUrgencyWeight +
            scheduleFit * UtilityScoringConfig.ScheduleFitWeight +
            distance * UtilityScoringConfig.DistanceWeight +
            personality * UtilityScoringConfig.PersonalityWeight;

        return new ActivityScore
        {
            Activity = activity,
            TotalScore = total,
            NeedUrgencyScore = needUrgency,
            ScheduleFitScore = scheduleFit,
            DistanceScore = distance,
            PersonalityScore = personality,
        };
    }

    private float GetFactorScore<TFactor>(NpcActivityType activity, in UtilityEvaluationContext context)
        where TFactor : IUtilityFactor
    {
        foreach (var factor in _factors)
        {
            if (factor is TFactor)
                return factor.Score(activity, in context);
        }

        return 0f;
    }

    private static bool ShouldSwitchActivity(
        NpcActivityType currentActivity,
        float currentScore,
        NpcActivityType challengerActivity,
        float challengerScore,
        double minutesInCurrentActivity)
    {
        if (challengerActivity == currentActivity)
            return false;

        var scoreDelta = challengerScore - currentScore;
        var marginMet = scoreDelta >= UtilityScoringConfig.ScoreMarginToSwitch;
        var dwellMet = minutesInCurrentActivity >= UtilityScoringConfig.MinActivityDurationGameMinutes &&
                       challengerScore > currentScore;

        return marginMet || dwellMet;
    }

    private static ActivityScore? GetRunnerUp(IReadOnlyList<ActivityScore> rankedScores, NpcActivityType selectedActivity)
    {
        return rankedScores.FirstOrDefault(score => score.Activity != selectedActivity);
    }
}