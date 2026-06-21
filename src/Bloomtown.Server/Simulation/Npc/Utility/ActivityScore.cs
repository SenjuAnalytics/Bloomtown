using Bloomtown.Server.Simulation.Npc.Schedule;

namespace Bloomtown.Server.Simulation.Npc.Utility;

/// <summary>
/// Utility score for one candidate activity, including per-factor breakdown.
/// </summary>
public sealed class ActivityScore
{
    public required NpcActivityType Activity { get; init; }
    public float TotalScore { get; init; }
    public float NeedUrgencyScore { get; init; }
    public float ScheduleFitScore { get; init; }
    public float DistanceScore { get; init; }
    public float PersonalityScore { get; init; }

    public string DescribeTopFactors()
    {
        var factors = new (string Name, float Score)[]
        {
            ("NeedUrgency", NeedUrgencyScore),
            ("ScheduleFit", ScheduleFitScore),
            ("Distance", DistanceScore),
            ("Personality", PersonalityScore),
        };

        var ranked = factors
            .OrderByDescending(pair => pair.Score)
            .Take(2)
            .Where(pair => pair.Score >= 35f)
            .Select(pair => DescribeFactor(pair.Name, pair.Score))
            .ToList();

        return ranked.Count > 0
            ? string.Join(" + ", ranked)
            : "balanced utility factors";
    }

    private static string DescribeFactor(string factorName, float score)
    {
        return factorName switch
        {
            "NeedUrgency" when score >= 70f => "need urgency is high",
            "NeedUrgency" => "need urgency is moderate",
            "ScheduleFit" when score >= 80f => "schedule fit is strong",
            "ScheduleFit" => "schedule fit is acceptable",
            "Distance" when score >= 70f => "destination is nearby",
            "Distance" => "destination distance is reasonable",
            "Personality" when score >= 70f => "personality preference",
            "Personality" => "personality alignment",
            _ => factorName,
        };
    }
}