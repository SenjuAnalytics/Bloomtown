namespace Bloomtown.Shared.Leadership;

/// <summary>
/// Human-readable labels for village leadership positions.
/// </summary>
public static class VillagePositionDisplay
{
    public static string GetName(VillagePosition position)
    {
        return position switch
        {
            VillagePosition.ProjectLeader => "Project Leader",
            VillagePosition.Advisor => "Village Advisor",
            VillagePosition.DeputyChief => "Deputy Village Chief",
            VillagePosition.Chief => "Village Chief",
            _ => "None",
        };
    }

    public static string GetSlug(VillagePosition position)
    {
        return position switch
        {
            VillagePosition.ProjectLeader => "project-leader",
            VillagePosition.Advisor => "advisor",
            VillagePosition.DeputyChief => "deputy",
            VillagePosition.Chief => "chief",
            _ => "none",
        };
    }

    public static bool TryParseSlug(string slug, out VillagePosition position)
    {
        position = slug.Trim().ToLowerInvariant() switch
        {
            "project-leader" or "projectleader" or "leader" => VillagePosition.ProjectLeader,
            "advisor" => VillagePosition.Advisor,
            "deputy" or "deputy-chief" => VillagePosition.DeputyChief,
            "chief" => VillagePosition.Chief,
            _ => VillagePosition.None,
        };

        return position != VillagePosition.None;
    }
}