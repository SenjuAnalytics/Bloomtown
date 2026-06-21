using Bloomtown.Shared.Protocol;

namespace Bloomtown.Client;

/// <summary>
/// Parses daily rhythm agency commands across morning, afternoon, and evening phases.
/// </summary>
public static class DailyRhythmCommandParser
{
    public static bool TryParse(string commandLine, out DailyRhythmRequest request, out string errorMessage)
    {
        request = default;
        errorMessage = string.Empty;

        var normalized = commandLine.Trim().ToLowerInvariant();
        if (normalized.StartsWith('/'))
            normalized = normalized[1..];

        if (string.IsNullOrWhiteSpace(normalized))
        {
            errorMessage = "Empty command.";
            return false;
        }

        if (normalized is "rhythm" or "daily rhythm")
        {
            request = new DailyRhythmRequest(DailyRhythmRequestKind.List);
            return true;
        }

        DailyRhythmRequestKind? kind = normalized switch
        {
            "start calm" or "start calmly" or "calm start" => DailyRhythmRequestKind.StartCalm,
            "start active" or "start actively" or "active start" => DailyRhythmRequestKind.StartActive,
            "focused break" or "take break" or "afternoon break" => DailyRhythmRequestKind.FocusedBreak,
            "rest early" or "rest tonight" => DailyRhythmRequestKind.RestEarly,
            "push through" or "keep going" => DailyRhythmRequestKind.PushThrough,
            "rhythm wind down" or "settle day" or "day wind down" => DailyRhythmRequestKind.WindDown,
            _ => null,
        };

        if (kind is null)
            return false;

        request = new DailyRhythmRequest(kind.Value);
        return true;
    }

    public static string BuildUsageHint() =>
        "Daily rhythm agency: rhythm | start calm | start active | focused break | rest early | push through | rhythm wind down";
}