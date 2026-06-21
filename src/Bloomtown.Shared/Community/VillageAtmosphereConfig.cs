using Bloomtown.Shared.Relationship;

namespace Bloomtown.Shared.Community;

/// <summary>
/// Village growth atmosphere: development level calculation, status flavor, and ambient dialogue.
/// </summary>
public static class VillageAtmosphereConfig
{
    /// <summary>All three built-in projects finished — village feels fully grown.</summary>
    public const int BustlingProjectThreshold = 3;

    /// <summary>At least one project finished — village no longer feels quiet.</summary>
    public const int LivelyProjectThreshold = 1;

    public const double QuietAmbientCheckIntervalSeconds = 8.0;
    public const double LivelyAmbientCheckIntervalSeconds = 6.0;
    public const double BustlingAmbientCheckIntervalSeconds = 5.0;

    public static readonly TimeSpan AtmosphereCommentCooldown = TimeSpan.FromMinutes(2.5);

    /// <summary>
    /// Derives development level from completed communal projects.
    /// 0 projects = Quiet, 1–2 = Lively, all 3 built-ins = Bustling.
    /// </summary>
    public static VillageDevelopmentLevel CalculateLevel(int completedProjectCount)
    {
        if (completedProjectCount >= BustlingProjectThreshold)
            return VillageDevelopmentLevel.Bustling;

        if (completedProjectCount >= LivelyProjectThreshold)
            return VillageDevelopmentLevel.Lively;

        return VillageDevelopmentLevel.Quiet;
    }

    public static string GetDisplayName(VillageDevelopmentLevel level) =>
        level switch
        {
            VillageDevelopmentLevel.Lively => "Lively",
            VillageDevelopmentLevel.Bustling => "Bustling",
            _ => "Quiet",
        };

    public static string GetStatusHeadline(VillageDevelopmentLevel level) =>
        level switch
        {
            VillageDevelopmentLevel.Bustling => "Village atmosphere: Bustling",
            VillageDevelopmentLevel.Lively => "Village atmosphere: Lively",
            _ => "Village atmosphere: Quiet",
        };

    public static string GetStatusFlavor(VillageDevelopmentLevel level) =>
        level switch
        {
            VillageDevelopmentLevel.Bustling =>
                "The village hums with activity — neighbors chatting, paths well worn, and improvements easy to spot.",
            VillageDevelopmentLevel.Lively =>
                "The village feels lively and busy compared to before — growth shows in daily life.",
            _ => "Bloomtown is peaceful and still finding its rhythm.",
        };

    public static double GetAmbientCheckIntervalSeconds(VillageDevelopmentLevel level) =>
        level switch
        {
            VillageDevelopmentLevel.Bustling => BustlingAmbientCheckIntervalSeconds,
            VillageDevelopmentLevel.Lively => LivelyAmbientCheckIntervalSeconds,
            _ => QuietAmbientCheckIntervalSeconds,
        };

    public static RelationshipTier GetMinAmbientCommentTier(VillageDevelopmentLevel level) =>
        level >= VillageDevelopmentLevel.Lively
            ? RelationshipTier.Acquaintance
            : RelationshipTier.Friend;

    /// <summary>
    /// Returns passive bonuses for the current village development level.
    /// Lively: slightly slower fatigue rise and gentle mood recovery.
    /// Bustling: stronger versions plus reduced mood decay under stress.
    /// </summary>
    public static VillageDevelopmentBonuses GetActiveBonuses(VillageDevelopmentLevel level) =>
        level switch
        {
            VillageDevelopmentLevel.Bustling => new VillageDevelopmentBonuses
            {
                FatigueRiseMultiplier = 0.85f,
                PassiveMoodRecoveryPerGameMinute = 0.06f,
                MoodDecayUnderStressMultiplier = 0.9f,
            },
            VillageDevelopmentLevel.Lively => new VillageDevelopmentBonuses
            {
                FatigueRiseMultiplier = 0.92f,
                PassiveMoodRecoveryPerGameMinute = 0.04f,
            },
            _ => VillageDevelopmentBonuses.None,
        };

    public static IReadOnlyList<string> FormatActiveBonusLines(VillageDevelopmentLevel level)
    {
        var bonuses = GetActiveBonuses(level);
        if (!bonuses.HasPassiveBonus)
            return [];

        return level switch
        {
            VillageDevelopmentLevel.Bustling =>
            [
                "Fatigue rises 15% slower while in the village",
                "Mood recovers gently when you are not stressed",
                "Mood holds up slightly better under stress",
            ],
            VillageDevelopmentLevel.Lively =>
            [
                "Fatigue rises 8% slower while in the village",
                "Mood recovers gently when you are not stressed",
            ],
            _ => [],
        };
    }

    /// <summary>Ambient lines that only appear once the village reaches Lively or Bustling.</summary>
    public static string? TryGetLevelExclusiveAmbientComment(
        VillageDevelopmentLevel level,
        uint variationSeed)
    {
        string[]? candidates = level switch
        {
            VillageDevelopmentLevel.Bustling =>
            [
                "Bloomtown feels alive today — you can sense the difference your projects made.",
                "I swear the whole village stands a little taller since we finished those builds.",
                "Even the air feels lighter now that everything's in place.",
                "Neighbors greet each other more often. That's growth you can feel.",
            ],
            VillageDevelopmentLevel.Lively =>
            [
                "The village has a warmer energy lately — have you noticed?",
                "Every finished project seems to bring a little more cheer to the streets.",
                "I catch myself smiling more just walking through town these days.",
                "Things are looking up around here. Slowly, but surely.",
            ],
            _ => null,
        };

        if (candidates is null || candidates.Length == 0)
            return null;

        return candidates[(int)(variationSeed % (uint)candidates.Length)];
    }

    /// <summary>General ambient lines that reflect how much the village has grown.</summary>
    public static string? TryGetGeneralAmbientComment(
        VillageDevelopmentLevel level,
        IReadOnlyCollection<byte> completedProjectIds,
        uint variationSeed)
    {
        if (level == VillageDevelopmentLevel.Quiet)
            return null;

        string[] candidates = level == VillageDevelopmentLevel.Bustling
            ? GetBustlingGeneralLines(completedProjectIds)
            : GetLivelyGeneralLines(completedProjectIds);

        if (candidates.Length == 0)
            return null;

        return candidates[(int)(variationSeed % (uint)candidates.Length)];
    }

    /// <summary>Short talk/greet flavor when the village has developed beyond Quiet.</summary>
    public static string? TryGetInteractionFlavor(
        VillageDevelopmentLevel level,
        NpcInteractionFlavorKind kind,
        uint variationSeed)
    {
        if (level == VillageDevelopmentLevel.Quiet)
            return null;

        string[]? candidates = kind switch
        {
            NpcInteractionFlavorKind.Greet when level == VillageDevelopmentLevel.Bustling =>
            [
                "The whole village seems awake today — good to see you out too.",
                "Everyone's been in better spirits since the latest projects wrapped up.",
            ],
            NpcInteractionFlavorKind.Greet =>
            [
                "Things feel a bit busier around town lately — nice, isn't it?",
                "The village has a warmer feel these days. Hello!",
            ],
            NpcInteractionFlavorKind.Talk when level == VillageDevelopmentLevel.Bustling =>
            [
                "With the well, bridge, and warehouse done, Bloomtown finally feels like home.",
                "I keep hearing folks praise the new improvements — makes me proud to live here.",
            ],
            NpcInteractionFlavorKind.Talk =>
            [
                "Have you noticed how the village changes after each project finishes?",
                "There's a bit more life in the streets since we finished those community builds.",
            ],
            _ => null,
        };

        if (candidates is null || candidates.Length == 0)
            return null;

        return candidates[(int)(variationSeed % (uint)candidates.Length)];
    }

    private static string[] GetLivelyGeneralLines(IReadOnlyCollection<byte> completedProjectIds)
    {
        if (completedProjectIds.Contains(VillageProjectBenefitConfig.WellProjectId))
        {
            return
            [
                "The new well brought everyone out this morning.",
                "I hear laughter near the village well again — lovely sound.",
                "Fresh water changed the mood around here.",
            ];
        }

        if (completedProjectIds.Contains(VillageProjectBenefitConfig.BridgeProjectId))
        {
            return
            [
                "Folks cross the bridge more often now that it's repaired.",
                "The river crossing feels safe again — what a difference.",
                "I saw neighbors meeting on the bridge earlier.",
            ];
        }

        if (completedProjectIds.Contains(VillageProjectBenefitConfig.WarehouseProjectId))
        {
            return
            [
                "The warehouse gives the village a proper heartbeat.",
                "Shared storage makes everyone feel a little more secure.",
                "I spotted helpers organizing supplies at the warehouse.",
            ];
        }

        return
        [
            "Bloomtown feels a touch busier than it used to.",
            "Something about the village feels warmer lately.",
            "I think we're building something special here, bit by bit.",
            "There's a hopeful feeling in the air since the village started growing.",
        ];
    }

    private static string[] GetBustlingGeneralLines(IReadOnlyCollection<byte> completedProjectIds)
    {
        return
        [
            "Bloomtown hardly feels like the quiet place it once was — in a good way.",
            "Between the well, the bridge, and the warehouse, there's always someone about.",
            "I love how the village keeps growing with every project we finish.",
            "Everyone seems to carry themselves a bit lighter since the village grew.",
            "You can hear friendly chatter from almost every corner now.",
            completedProjectIds.Contains(VillageProjectBenefitConfig.WellProjectId)
                ? "Neighbors gather at the well like it's the heart of town."
                : "The streets feel alive today.",
        ];
    }
}

/// <summary>Used by VillageAtmosphereConfig for talk/greet flavor selection.</summary>
public enum NpcInteractionFlavorKind : byte
{
    Greet = 0,
    Talk = 1,
}