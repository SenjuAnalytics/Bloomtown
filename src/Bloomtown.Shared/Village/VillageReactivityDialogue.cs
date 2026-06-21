using Bloomtown.Shared.Community;
using Bloomtown.Shared.Protocol;
using Bloomtown.Shared.World;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Growth reaction and project-completion flavor for visible village development.
/// </summary>
internal static class VillageReactivityDialogue
{
    internal static string? TryGetLivelyGrowthLine(
        IReadOnlyCollection<byte> completedProjectIds,
        uint variationSeed)
    {
        var lines = new List<string>
        {
            "The village feels lively — you can hear the difference since the projects went in.",
            "Something about Bloomtown feels warmer lately. I think the improvements show.",
            "More neighbors out and about today. Growth looks good on this place.",
            "I keep noticing little changes around town — all of them for the better.",
        };

        AppendProjectGrowthLines(lines, completedProjectIds, VillageDevelopmentLevel.Lively);

        if (lines.Count == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Count)];
    }

    internal static string? TryGetBustlingGrowthLine(
        IReadOnlyCollection<byte> completedProjectIds,
        uint variationSeed)
    {
        var lines = new List<string>
        {
            "Bloomtown hardly sits still anymore — busy in the best way.",
            "Every lane feels worn in now, like a real village should.",
            "I swear you can feel the village humming. That's what finishing those builds did.",
            "New improvements are visible everywhere you look — hard to miss, easy to love.",
        };

        AppendProjectGrowthLines(lines, completedProjectIds, VillageDevelopmentLevel.Bustling);

        if (lines.Count == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Count)];
    }

    internal static string? TryGetProjectCompletionReaction(
        byte projectId,
        VillageDevelopmentLevel developmentLevel,
        uint variationSeed,
        out uint preferredSpeakerNpcEntityId)
    {
        preferredSpeakerNpcEntityId = VillageReactivityConfig.SelectProjectReactionSpeaker(projectId, variationSeed);
        var lines = GetCompletionReactionLines(projectId, developmentLevel);
        if (lines.Length == 0)
            return null;

        var entry = lines[(int)(variationSeed % (uint)lines.Length)];
        preferredSpeakerNpcEntityId = entry.SpeakerNpcEntityId;
        return entry.Text;
    }

    internal static string? TryGetProjectSiteGrowthComment(
        byte projectId,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay,
        uint variationSeed)
    {
        var lines = GetProjectSiteGrowthLines(projectId, developmentLevel, timeOfDay);
        if (lines.Length == 0)
            return null;

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }

    private static void AppendProjectGrowthLines(
        List<string> lines,
        IReadOnlyCollection<byte> completedProjectIds,
        VillageDevelopmentLevel developmentLevel)
    {
        if (completedProjectIds.Contains(VillageProjectBenefitConfig.WellProjectId))
        {
            lines.Add(developmentLevel == VillageDevelopmentLevel.Bustling
                ? "The well draws a crowd morning and night — the village heart is obvious now."
                : "Fresh water changed the rhythm here. You notice it the moment you pass the well.");
        }

        if (completedProjectIds.Contains(VillageProjectBenefitConfig.BridgeProjectId))
        {
            lines.Add(developmentLevel == VillageDevelopmentLevel.Bustling
                ? "Footsteps on the bridge never seem to stop — connection you can hear."
                : "The repaired bridge made both sides of town feel closer overnight.");
        }

        if (completedProjectIds.Contains(VillageProjectBenefitConfig.WarehouseProjectId))
        {
            lines.Add(developmentLevel == VillageDevelopmentLevel.Bustling
                ? "The warehouse keeps everyone supplied — you feel that stability walking past."
                : "Shared storage gave the village a backbone you can actually see in daily life.");
        }
    }

    private static CompletionReactionEntry[] GetCompletionReactionLines(
        byte projectId,
        VillageDevelopmentLevel developmentLevel)
    {
        return projectId switch
        {
            VillageProjectBenefitConfig.WellProjectId => developmentLevel switch
            {
                VillageDevelopmentLevel.Bustling =>
                [
                    new(NpcEntityIds.Tom, "The well's finished and the whole village knows it — best water we've had in years."),
                    new(NpcEntityIds.Elsie, "I still smile when I see neighbors lining up at the well. That never used to happen."),
                ],
                VillageDevelopmentLevel.Lively =>
                [
                    new(NpcEntityIds.Elsie, "The well is done — you can already feel mornings change around here."),
                    new(NpcEntityIds.Tom, "Fresh water at the well again. Hard not to notice the difference."),
                ],
                _ =>
                [
                    new(NpcEntityIds.Elsie, "The well is finished — thank everyone who pitched in."),
                    new(NpcEntityIds.Tom, "Water at the well again. Small build, big relief."),
                ],
            },

            VillageProjectBenefitConfig.BridgeProjectId => developmentLevel switch
            {
                VillageDevelopmentLevel.Bustling =>
                [
                    new(NpcEntityIds.Elsie, "The bridge is back and the village moves twice as freely — you can see it from here."),
                    new(NpcEntityIds.Tom, "I watched folks cross all day after we mended the bridge. That's growth you can point to."),
                ],
                VillageDevelopmentLevel.Lively =>
                [
                    new(NpcEntityIds.Tom, "Bridge repairs are done — crossing feels safe again, finally."),
                    new(NpcEntityIds.Elsie, "The repaired bridge changed how neighbors visit each other. Lovely to witness."),
                ],
                _ =>
                [
                    new(NpcEntityIds.Tom, "The bridge is sturdy again — safe crossing for everyone."),
                    new(NpcEntityIds.Elsie, "We can cross the river properly again. The village feels less cut off."),
                ],
            },

            VillageProjectBenefitConfig.WarehouseProjectId => developmentLevel switch
            {
                VillageDevelopmentLevel.Bustling =>
                [
                    new(NpcEntityIds.Tom, "Warehouse's open and the village finally feels stocked for whatever comes next."),
                    new(NpcEntityIds.Elsie, "Helpers still buzz around the warehouse — finishing it left a mark you can hear."),
                ],
                VillageDevelopmentLevel.Lively =>
                [
                    new(NpcEntityIds.Elsie, "The warehouse is done — supplies feel steadier already."),
                    new(NpcEntityIds.Tom, "Shared storage changes the mood. Everyone seems a little less worried."),
                ],
                _ =>
                [
                    new(NpcEntityIds.Elsie, "The warehouse is finished — our harvest has a proper home now."),
                    new(NpcEntityIds.Tom, "Extra storage means the village can plan ahead. Good work."),
                ],
            },

            _ => [],
        };
    }

    private static string[] GetProjectSiteGrowthLines(
        byte projectId,
        VillageDevelopmentLevel developmentLevel,
        GameTimeOfDay timeOfDay)
    {
        var baseLines = projectId switch
        {
            VillageProjectBenefitConfig.WellProjectId => developmentLevel switch
            {
                VillageDevelopmentLevel.Bustling =>
                [
                    "The well never seems quiet anymore — neighbors, buckets, easy laughter.",
                    "You can tell this spot became the village center after the well went in.",
                    "Morning light on the well just feels different now — busier, friendlier.",
                ],
                VillageDevelopmentLevel.Lively =>
                [
                    "The finished well changed how this corner of town sounds in the morning.",
                    "Fresh water here still feels like a small miracle when you stand this close.",
                ],
                _ =>
                [
                    "Standing here, you remember why finishing the well mattered.",
                ],
            },

            VillageProjectBenefitConfig.BridgeProjectId => developmentLevel switch
            {
                VillageDevelopmentLevel.Bustling =>
                [
                    "Crossings come one after another — the bridge wears its repairs well.",
                    "From the bridge you can see both sides of Bloomtown stirring at once.",
                    "River breeze and steady footsteps — this crossing feels alive again.",
                ],
                VillageDevelopmentLevel.Lively =>
                [
                    "The repaired planks underfoot make the whole crossing feel trustworthy.",
                    "More people take this route now that the bridge is safe again.",
                ],
                _ =>
                [
                    "The bridge feels solid again — a quiet sign the village is moving forward.",
                ],
            },

            VillageProjectBenefitConfig.WarehouseProjectId => developmentLevel switch
            {
                VillageDevelopmentLevel.Bustling =>
                [
                    "Crates move in and out all day — the warehouse keeps the village supplied.",
                    "This place hums with purpose now that storage is shared and steady.",
                    "You can hear helpers inside sorting the afternoon delivery.",
                ],
                VillageDevelopmentLevel.Lively =>
                [
                    "The warehouse already looks like it belongs at the center of village life.",
                    "Shared supplies here made everyone walk a little lighter past the doors.",
                ],
                _ =>
                [
                    "The warehouse stands ready — proof the village can finish what it starts.",
                ],
            },

            _ => Array.Empty<string>(),
        };

        if (baseLines.Length == 0)
            return baseLines;

        if (timeOfDay == GameTimeOfDay.Evening && developmentLevel >= VillageDevelopmentLevel.Lively)
        {
            return baseLines.Concat(projectId switch
            {
                VillageProjectBenefitConfig.WellProjectId => ["Evening trips to the well feel unhurried and familiar now."],
                VillageProjectBenefitConfig.BridgeProjectId => ["At dusk the bridge catches the last warm light — worth pausing for."],
                VillageProjectBenefitConfig.WarehouseProjectId => ["Supper hour feels calmer knowing the stores are stacked and secure."],
                _ => [],
            }).ToArray();
        }

        return baseLines;
    }

    private readonly record struct CompletionReactionEntry(uint SpeakerNpcEntityId, string Text);
}