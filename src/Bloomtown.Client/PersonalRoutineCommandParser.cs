using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.Routines;

namespace Bloomtown.Client;

/// <summary>
/// Parses personal routine commands: routines, morning stretch, evening wind down, sit and reflect.
/// </summary>
public static class PersonalRoutineCommandParser
{
    public static bool TryParse(string commandLine, out PersonalRoutineRequest request, out string errorMessage)
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

        if (normalized is "routines" or "routine" or "personal routines" or "personal routine")
        {
            request = new PersonalRoutineRequest(PersonalRoutineRequestKind.List, PersonalRoutineKind.None);
            return true;
        }

        var routine = normalized switch
        {
            "morning stretch" or "stretch" => PersonalRoutineKind.MorningStretch,
            "evening wind down" or "wind down" => PersonalRoutineKind.EveningWindDown,
            "sit and reflect" or "personal reflect" => PersonalRoutineKind.SitAndReflect,
            _ => PersonalRoutineKind.None,
        };

        if (routine == PersonalRoutineKind.None)
            return false;

        request = new PersonalRoutineRequest(PersonalRoutineRequestKind.Perform, routine);
        return true;
    }

    public static string BuildUsageHint() =>
        "Personal routines: routines | morning stretch | evening wind down | sit and reflect";
}