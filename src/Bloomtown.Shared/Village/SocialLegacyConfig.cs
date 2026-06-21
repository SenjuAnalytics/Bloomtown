using Bloomtown.Shared.Protocol;

namespace Bloomtown.Shared.Village;

/// <summary>
/// Social milestones and light Well-liked legacy effects — a long-term journey through Bloomtown,
/// not a stack of tier bonuses.
/// </summary>
public static class SocialLegacyConfig
{
    public const int MinFocusCloseFriendsForKnownMilestone = 2;
    public const int MinFocusCloseFriendsForTrustedMilestone = 3;

    public const float PassiveMoodRecoveryPerGameMinute = 0.16f;
    public const int AmbientChanceBonusPercent = 8;
    public const int AmbientCooldownReductionGameMinutes = 5;

    public const int LegacyNpcMentionChancePercent = 22;
    public const int LegacyNpcMentionCooldownGameMinutes = 38;

    public const int LegacyAmbientCommentChancePercent = 18;
    public const int LegacyContributionNarrativeChancePercent = 36;

    public const int VillagePillarAcknowledgmentChancePercent = 38;

    public static bool IsLegacyActive(VillageSocialStandingTier tier) =>
        tier >= VillageSocialStandingTier.WellLiked;

    public static bool HasMilestone(
        SocialMilestoneKind milestone,
        VillageSocialStandingTier tier,
        int focusCloseFriendCount)
    {
        return milestone switch
        {
            SocialMilestoneKind.KnownInBloomtown =>
                tier >= VillageSocialStandingTier.Respected
                    && focusCloseFriendCount >= MinFocusCloseFriendsForKnownMilestone,
            SocialMilestoneKind.TrustedNeighbor =>
                tier >= VillageSocialStandingTier.WellLiked
                    && focusCloseFriendCount >= MinFocusCloseFriendsForTrustedMilestone,
            SocialMilestoneKind.VillagePillar =>
                IsLegacyActive(tier)
                    && focusCloseFriendCount >= MinFocusCloseFriendsForTrustedMilestone,
            _ => false,
        };
    }

    public static IReadOnlyList<SocialMilestoneKind> GetEarnedMilestones(
        VillageSocialStandingTier tier,
        int focusCloseFriendCount)
    {
        var milestones = new List<SocialMilestoneKind>(3);
        if (HasMilestone(SocialMilestoneKind.KnownInBloomtown, tier, focusCloseFriendCount))
            milestones.Add(SocialMilestoneKind.KnownInBloomtown);
        if (HasMilestone(SocialMilestoneKind.TrustedNeighbor, tier, focusCloseFriendCount))
            milestones.Add(SocialMilestoneKind.TrustedNeighbor);
        if (HasMilestone(SocialMilestoneKind.VillagePillar, tier, focusCloseFriendCount))
            milestones.Add(SocialMilestoneKind.VillagePillar);
        return milestones;
    }

    public static string GetMilestoneDisplayName(SocialMilestoneKind milestone) =>
        milestone switch
        {
            SocialMilestoneKind.KnownInBloomtown => "Known in Bloomtown",
            SocialMilestoneKind.TrustedNeighbor => "Trusted Neighbor",
            SocialMilestoneKind.VillagePillar => "Village Pillar",
            _ => string.Empty,
        };

    public static string GetMilestoneDescription(SocialMilestoneKind milestone) =>
        milestone switch
        {
            SocialMilestoneKind.KnownInBloomtown =>
                "Neighbors recognize your place here — Bloomtown knows your name.",
            SocialMilestoneKind.TrustedNeighbor =>
                "Bloomtown holds you among its most trusted residents.",
            SocialMilestoneKind.VillagePillar =>
                "You stand among Bloomtown's pillars — your name is woven into village life, "
                + "and legacy effects carry the weight of years well spent.",
            _ => string.Empty,
        };

    public static int ApplyAmbientChanceBonus(int baseChance, VillageSocialStandingTier tier) =>
        IsLegacyActive(tier)
            ? baseChance + AmbientChanceBonusPercent
            : baseChance;

    public static int ApplyAmbientCooldownReduction(int baseCooldownMinutes, VillageSocialStandingTier tier)
    {
        if (!IsLegacyActive(tier))
            return baseCooldownMinutes;

        var reduced = baseCooldownMinutes - AmbientCooldownReductionGameMinutes;
        return reduced > 0 ? reduced : baseCooldownMinutes;
    }

    public static float GetPassiveMoodRecoveryPerGameMinute(VillageSocialStandingTier tier) =>
        IsLegacyActive(tier) ? PassiveMoodRecoveryPerGameMinute : 0f;

    public static bool IsEligibleForLegacyNpcMention(uint npcEntityId) =>
        npcEntityId is NpcEntityIds.Harold or NpcEntityIds.Greta or NpcEntityIds.Elsie or NpcEntityIds.Rowan or NpcEntityIds.Marcus or NpcEntityIds.Eleanor;

    public static bool IsEligibleForVillagePillarAcknowledgment(uint npcEntityId) =>
        npcEntityId is NpcEntityIds.Harold or NpcEntityIds.Greta or NpcEntityIds.Elsie or NpcEntityIds.Rowan
            or NpcEntityIds.Marcus or NpcEntityIds.Eleanor;

    public static bool ShouldTriggerLegacyNpcMention(
        uint playerEntityId,
        uint npcEntityId,
        VillageSocialStandingTier tier,
        long totalGameMinutes,
        uint variationSeed)
    {
        if (!IsLegacyActive(tier) || !IsEligibleForLegacyNpcMention(npcEntityId))
            return false;

        var roll = (playerEntityId * 137 + npcEntityId * 101 + variationSeed * 67 + (uint)(totalGameMinutes % 953)) % 100;
        return roll < LegacyNpcMentionChancePercent;
    }

    public static bool ShouldUseVillagePillarAcknowledgment(
        uint playerEntityId,
        uint npcEntityId,
        long totalGameMinutes,
        uint variationSeed)
    {
        if (!IsEligibleForVillagePillarAcknowledgment(npcEntityId))
            return false;

        var roll = (playerEntityId * 157 + npcEntityId * 109 + variationSeed * 73 + (uint)(totalGameMinutes % 977)) % 100;
        return roll < VillagePillarAcknowledgmentChancePercent;
    }

    public static bool ShouldUseLegacyAmbientComment(
        uint playerEntityId,
        VillageSocialStandingTier tier,
        long totalGameMinutes,
        uint attemptCounter)
    {
        if (!IsLegacyActive(tier))
            return false;

        var roll = (playerEntityId * 139 + (uint)(totalGameMinutes % 967) + attemptCounter * 19) % 100;
        return roll < LegacyAmbientCommentChancePercent;
    }

    public static bool ShouldShowContributionLegacyFeedback(
        uint playerEntityId,
        byte projectId,
        long totalGameMinutes,
        uint variationSeed)
    {
        var roll = (playerEntityId * 149 + projectId * 103 + variationSeed * 71 + (uint)(totalGameMinutes % 971)) % 100;
        return roll < LegacyContributionNarrativeChancePercent;
    }

    public static string? FormatMilestonesStatus(
        VillageSocialStandingTier tier,
        int focusCloseFriendCount)
    {
        var milestones = GetEarnedMilestones(tier, focusCloseFriendCount);
        if (milestones.Count == 0)
            return null;

        var lines = new List<string> { "Milestones earned:" };
        foreach (var milestone in milestones)
        {
            var prefix = milestone == SocialMilestoneKind.VillagePillar ? "  ★ " : "  ✓ ";
            lines.Add(
                $"{prefix}{GetMilestoneDisplayName(milestone)} — {GetMilestoneDescription(milestone)}");
        }

        return string.Join(Environment.NewLine, lines);
    }

    public static string? FormatLegacyEffectsStatus(
        VillageSocialStandingTier tier,
        bool villageNoticedMemory,
        int focusCloseFriendCount = 0)
    {
        if (!IsLegacyActive(tier))
            return null;

        var ambientChance = VillageSocialStandingConfig.GetAmbientChancePercent(tier, villageNoticedMemory);
        var ambientCooldown = VillageSocialStandingConfig.GetAmbientCooldownGameMinutes(tier);
        var hasVillagePillar = HasMilestone(
            SocialMilestoneKind.VillagePillar,
            tier,
            focusCloseFriendCount);

        var lines = new List<string>
        {
            "── Social Legacy (Well-liked, active) ──",
            "Your long journey here is recognized — Bloomtown carries your name with quiet pride.",
            string.Empty,
            "Active effects:",
            $"  • Passive mood: gentle steadiness while walking the village (~+{PassiveMoodRecoveryPerGameMinute:F2}/game minute).",
            $"  • Ambient recognition: villagers mention your reputation more often (~{ambientChance}% chance, ~{ambientCooldown}m cooldown).",
            $"  • Long journey: Harold, Greta, Elsie, Rowan, Marcus, and Eleanor may acknowledge how deeply you've rooted here (~{LegacyNpcMentionChancePercent}% chance, ~{LegacyNpcMentionCooldownGameMinutes}m per NPC).",
            $"  • Shared work: legacy weight on community contributions (~{LegacyContributionNarrativeChancePercent}% narrative chance).",
        };

        if (hasVillagePillar)
        {
            lines.Add(string.Empty);
            lines.Add(
                "  ★ Village Pillar: Harold, Greta, Rowan, Marcus, and Eleanor may speak of you "
                + "as one of the pillars Bloomtown is built upon.");
        }

        return string.Join(Environment.NewLine, lines);
    }

    public static string? FormatLegacyEffectsStatusCompact(
        VillageSocialStandingTier tier,
        bool villageNoticedMemory,
        int focusCloseFriendCount = 0)
    {
        if (!IsLegacyActive(tier))
            return null;

        var ambientChance = VillageSocialStandingConfig.GetAmbientChancePercent(tier, villageNoticedMemory);
        var ambientCooldown = VillageSocialStandingConfig.GetAmbientCooldownGameMinutes(tier);
        var hasVillagePillar = HasMilestone(
            SocialMilestoneKind.VillagePillar,
            tier,
            focusCloseFriendCount);

        var line =
            $"  ★ Legacy active: mood +{PassiveMoodRecoveryPerGameMinute:F2}/min in village · "
            + $"ambient ~{ambientChance}% / {ambientCooldown}m · "
            + $"journey lines ~{LegacyNpcMentionChancePercent}% / {LegacyNpcMentionCooldownGameMinutes}m per NPC";

        if (hasVillagePillar)
            line += " · Village Pillar acknowledgments enabled";

        return line;
    }

    public static string FormatLegacyNpcMentionFeedback(string npcDisplayName, string journeyLine) =>
        $"[Social legacy — {journeyLine}]";

    public static string FormatVillagePillarAcknowledgmentFeedback(string npcDisplayName, string acknowledgmentLine) =>
        $"[Village Pillar — {acknowledgmentLine}]";

    public static string FormatContributionLegacyFeedback(uint variationSeed)
    {
        string[] lines =
        [
            "[Social legacy — Bloomtown remembers your long path; your standing lends quiet weight to shared work.]",
            "[Social legacy — years of trust follow you to the workbench; neighbors notice when someone like you lends a hand.]",
            "[Social legacy — the village carries your name into this project; communal work feels a little more certain.]",
            "[Social legacy — folk speak of your journey with fondness; your contribution lands with real weight here.]",
            "[Social legacy — not louder work, but work the whole village quietly believes in.]",
        ];

        return lines[(int)(variationSeed % (uint)lines.Length)];
    }
}